using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using BoraLive.Application.Abstractions;
using BoraLive.Application.ApiKeys;
using BoraLive.Application.Art;
using BoraLive.Domain.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BoraLive.Infrastructure.Gemini;

/// <summary>
/// Implementação da porta <see cref="IAiImageGenerator"/> que conversa com o Gemini.
/// </summary>
// O HttpClient chega pronto pela injeção de dependência ("HttpClient tipado"),
// já com o endereço base e o tempo limite definidos em AddInfrastructure().
public sealed class GeminiImageGenerator(
    HttpClient httpClient,
    IOptions<GeminiOptions> options,
    ILogger<GeminiImageGenerator> logger) : IAiImageGenerator
{
    /// <summary>Header em que o Gemini espera a chave.</summary>
    public const string ApiKeyHeader = "x-goog-api-key";

    // Listar modelos é a chamada mais barata que exige uma chave válida:
    // não gera nada e não consome cota de geração.
    private const string ListModelsPath = "v1beta/models?pageSize=1";

    // Proporções que o Gemini sabe gerar, como largura/altura.
    // Nenhuma é igual ao banner (2,5:1): a mais próxima é usada e o Skia corta o excesso.
    private static readonly (string Name, double Ratio)[] SupportedAspectRatios =
    [
        ("1:1", 1.0), ("2:3", 2.0 / 3), ("3:2", 3.0 / 2), ("3:4", 3.0 / 4), ("4:3", 4.0 / 3),
        ("4:5", 4.0 / 5), ("5:4", 5.0 / 4), ("9:16", 9.0 / 16), ("16:9", 16.0 / 9), ("21:9", 21.0 / 9),
    ];

    private readonly GeminiOptions _options = options.Value;

    public async Task<ApiKeyStatus> ValidateKeyAsync(string apiKey, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, ListModelsPath);

        // SEGURANÇA: a chave vai no header, nunca na URL.
        // O HttpClient escreve a URL das requisições no log, mas não os headers.
        request.Headers.TryAddWithoutValidation(ApiKeyHeader, apiKey);

        // O HttpClient espera até o limite maior (geração); aqui vale o limite curto da validação.
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

        try
        {
            using var response = await httpClient.SendAsync(request, timeout.Token);

            return response.StatusCode switch
            {
                HttpStatusCode.OK => ApiKeyStatus.Valid,

                // O Gemini responde 400 para chave inexistente e 401/403 para chave sem permissão.
                HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
                    => ApiKeyStatus.Invalid,

                // 429 (limite de chamadas), 5xx e qualquer outra coisa: não dá para afirmar nada.
                _ => ApiKeyStatus.Unavailable,
            };
        }
        catch (HttpRequestException)
        {
            // Falha de rede: sem internet, DNS, conexão recusada.
            return ApiKeyStatus.Unavailable;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Estourou o tempo limite (e não foi o usuário que cancelou).
            return ApiKeyStatus.Unavailable;
        }
    }

    public async Task<Result<byte[]>> GenerateBackgroundAsync(
        string apiKey,
        string prompt,
        int width,
        int height,
        CancellationToken cancellationToken)
    {
        var path = $"v1beta/models/{Uri.EscapeDataString(_options.ImageModel)}:generateContent";

        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(new GenerateRequest(
                [new Content([new Part(prompt)])],
                new GenerationConfig(["IMAGE"], new ImageConfig(NearestAspectRatio(width, height))))),
        };

        // SEGURANÇA: igual à validação, a chave só vai no header.
        request.Headers.TryAddWithoutValidation(ApiKeyHeader, apiKey);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(_options.ImageTimeoutSeconds));

        try
        {
            using var response = await httpClient.SendAsync(request, timeout.Token);

            if (!response.IsSuccessStatusCode)
            {
                return await ErrorFromAsync(response, timeout.Token);
            }

            var body = await response.Content.ReadFromJsonAsync<GenerateResponse>(timeout.Token);

            var image = body?.Candidates?
                .SelectMany(candidate => candidate.Content?.Parts ?? [])
                .Select(part => part.InlineData)
                .FirstOrDefault(data => !string.IsNullOrEmpty(data?.Data));

            // Resposta 200 sem imagem: o Gemini recusou por segurança ou respondeu só com texto.
            if (image is null)
            {
                logger.LogWarning("Gemini respondeu 200, mas sem imagem (provável bloqueio de segurança).");
                return ArtErrors.AiRejected;
            }

            return Convert.FromBase64String(image.Data!);
        }
        catch (Exception exception) when (
            exception is HttpRequestException or FormatException or System.Text.Json.JsonException
            || (exception is OperationCanceledException && !cancellationToken.IsCancellationRequested))
        {
            // Rede caiu, tempo esgotado, ou resposta que não dá para entender: tudo vira "indisponível".
            // Loga só o tipo da exceção: nunca a chave (ela só existe no header, que não vai para o log).
            logger.LogWarning("Falha ao gerar fundo no Gemini: {ExceptionType}.", exception.GetType().Name);
            return ArtErrors.AiUnavailable;
        }
    }

    private async Task<Error> ErrorFromAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        // Sem isto, "a IA não respondeu" esconde se foi modelo inexistente (404), cota (429) ou queda (5xx).
        logger.LogWarning(
            "Gemini recusou a geração de imagem: HTTP {StatusCode} ({Reason}).",
            (int)response.StatusCode,
            SummarizeGeminiError(body));

        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden:
                return ApiKeyErrors.Invalid;

            case HttpStatusCode.BadRequest:
                // O 400 vale para chave inexistente e para pedido recusado. O corpo diferencia os dois.
                return body.Contains("API_KEY_INVALID", StringComparison.Ordinal)
                    ? ApiKeyErrors.Invalid
                    : ArtErrors.AiRejected;

            default:
                // 404 (modelo), 429 (cota), 5xx e o resto: o serviço não conseguiu agora.
                return ArtErrors.AiUnavailable;
        }
    }

    /// <summary>Extrai "STATUS: mensagem" do JSON de erro do Gemini, curto o bastante para um log.</summary>
    private static string SummarizeGeminiError(string body)
    {
        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(body);
            var error = document.RootElement.GetProperty("error");
            var status = error.TryGetProperty("status", out var s) ? s.GetString() : null;
            var message = error.TryGetProperty("message", out var m) ? m.GetString() : null;
            var summary = $"{status}: {message}";

            return summary.Length <= 300 ? summary : summary[..300];
        }
        catch (Exception exception) when (exception is System.Text.Json.JsonException or KeyNotFoundException
                                              or InvalidOperationException)
        {
            return "corpo de erro ilegível";
        }
    }

    /// <summary>Escolhe, entre as proporções suportadas, a mais parecida com a da arte.</summary>
    private static string NearestAspectRatio(int width, int height)
    {
        var target = (double)width / height;

        // Compara na escala logarítmica: "o dobro" e "a metade" ficam igualmente distantes de 1.
        return SupportedAspectRatios
            .MinBy(option => Math.Abs(Math.Log(option.Ratio / target)))
            .Name;
    }

    // ---------- Formato JSON do Gemini (só o que usamos) ----------

    private sealed record GenerateRequest(
        [property: JsonPropertyName("contents")] Content[] Contents,
        [property: JsonPropertyName("generationConfig")] GenerationConfig GenerationConfig);

    private sealed record Content(
        [property: JsonPropertyName("parts")] Part[] Parts);

    private sealed record Part(
        [property: JsonPropertyName("text")] string Text);

    private sealed record GenerationConfig(
        [property: JsonPropertyName("responseModalities")] string[] ResponseModalities,
        [property: JsonPropertyName("imageConfig")] ImageConfig ImageConfig);

    private sealed record ImageConfig(
        [property: JsonPropertyName("aspectRatio")] string AspectRatio);

    private sealed record GenerateResponse(
        [property: JsonPropertyName("candidates")] Candidate[]? Candidates);

    private sealed record Candidate(
        [property: JsonPropertyName("content")] ResponseContent? Content);

    private sealed record ResponseContent(
        [property: JsonPropertyName("parts")] ResponsePart[]? Parts);

    private sealed record ResponsePart(
        [property: JsonPropertyName("inlineData")] InlineData? InlineData);

    private sealed record InlineData(
        [property: JsonPropertyName("data")] string? Data);
}
