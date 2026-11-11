using System.Net;
using BoraLive.Application.Abstractions;

namespace BoraLive.Infrastructure.Gemini;

/// <summary>
/// Implementação da porta <see cref="IAiImageGenerator"/> que conversa com o Gemini.
/// </summary>
// O HttpClient chega pronto pela injeção de dependência ("HttpClient tipado"),
// já com o endereço base e o tempo limite definidos em AddInfrastructure().
public sealed class GeminiImageGenerator(HttpClient httpClient) : IAiImageGenerator
{
    /// <summary>Header em que o Gemini espera a chave.</summary>
    public const string ApiKeyHeader = "x-goog-api-key";

    // Listar modelos é a chamada mais barata que exige uma chave válida:
    // não gera nada e não consome cota de geração.
    private const string ListModelsPath = "v1beta/models?pageSize=1";

    public async Task<ApiKeyStatus> ValidateKeyAsync(string apiKey, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, ListModelsPath);

        // SEGURANÇA: a chave vai no header, nunca na URL.
        // O HttpClient escreve a URL das requisições no log, mas não os headers.
        request.Headers.TryAddWithoutValidation(ApiKeyHeader, apiKey);

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);

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
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Estourou o tempo limite do HttpClient (e não foi o usuário que cancelou).
            return ApiKeyStatus.Unavailable;
        }
    }
}
