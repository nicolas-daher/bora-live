using System.Buffers.Binary;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BoraLive.Api.Tests.Support;
using BoraLive.Application.Art;
using BoraLive.Application.Formats;
using BoraLive.Application.Templates;

namespace BoraLive.Api.Tests;

// Estes testes desenham de verdade, com o SkiaSharp e as fontes embutidas.
// Rodam igual no Windows e no Linux do CI.
public sealed class ArtEndpointTests(BoraLiveApiFactory factory) : IClassFixture<BoraLiveApiFactory>
{
    // Um PNG válido de 1x1 pixel, para servir de logo.
    private static readonly byte[] TinyPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");

    private const string SecretKey = "AIzaSy-CHAVE_SECRETA_DE_TESTE.9f3b7c";

    private readonly HttpClient _client = factory.CreateClient();

    // O xUnit cria uma instância da classe por teste: aqui o Gemini simulado volta ao zero.
    private readonly FakeGeminiHandler _gemini = ResetGemini(factory);

    private static FakeGeminiHandler ResetGemini(BoraLiveApiFactory factory)
    {
        factory.Gemini.Reset();
        return factory.Gemini;
    }

    // Resposta do Gemini com uma imagem dentro (aqui, o mesmo PNG de 1x1 pixel).
    private static string GeminiImageResponse() =>
        $$$"""{"candidates":[{"content":{"parts":[{"inlineData":{"mimeType":"image/png","data":"{{{Convert.ToBase64String(TinyPng)}}}"}}]}}]}""";

    private async Task<HttpResponseMessage> PostArtAsync(
        string templateId = "cs2-tatico",
        string format = "thumbnail",
        string streamerName = "mare_alta",
        string liveTitle = "Subindo de elo, sem tilt",
        byte[]? logo = null,
        string? prompt = null,
        string? apiKey = null)
    {
        // MultipartFormDataContent monta o mesmo corpo que um formulário com anexo do navegador.
        var form = new MultipartFormDataContent
        {
            { new StringContent(templateId), "templateId" },
            { new StringContent(format), "format" },
            { new StringContent(streamerName), "streamerName" },
            { new StringContent(liveTitle), "liveTitle" },
        };

        if (prompt is not null)
        {
            form.Add(new StringContent(prompt), "prompt");
        }

        if (logo is not null)
        {
            var file = new ByteArrayContent(logo);
            file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            form.Add(file, "logo", "logo.png");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/art") { Content = form };

        if (apiKey is not null)
        {
            request.Headers.Add("X-Gemini-Api-Key", apiKey);
        }

        // await: o "using" acima só pode descartar a requisição depois que o envio termina.
        return await _client.SendAsync(request);
    }

    private static string? WarningOf(HttpResponseMessage response) =>
        response.Headers.TryGetValues("X-Bora-Warning", out var values) ? values.Single() : null;

    // Todo PNG começa com a mesma assinatura e guarda largura e altura
    // nos bytes 16 a 23. Dá para conferir o tamanho sem biblioteca de imagem.
    private static (int Width, int Height) ReadPngSize(byte[] png)
    {
        byte[] signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        Assert.Equal(signature, png[..8]);

        return (
            BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4)),
            BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4)));
    }

    private static async Task<string?> ReadErrorCodeAsync(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("code").GetString();
    }

    [Fact]
    public async Task PostArt_EveryTemplateInEveryFormat_ReturnsPngWithTheExactSize()
    {
        var templates = await _client.GetFromJsonAsync<TemplateDto[]>("/api/templates");
        var formats = await _client.GetFromJsonAsync<FormatDto[]>("/api/formats");

        foreach (var template in templates!)
        {
            foreach (var formatKey in template.Formats)
            {
                var expected = formats!.Single(format => format.Key == formatKey);

                var response = await PostArtAsync(template.Id, formatKey, logo: TinyPng);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);

                var size = ReadPngSize(await response.Content.ReadAsByteArrayAsync());
                Assert.Equal((expected.Width, expected.Height), size);
            }
        }
    }

    [Fact]
    public async Task PostArt_WithoutLogo_StillReturnsPng()
    {
        var response = await PostArtAsync(logo: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal((1280, 720), ReadPngSize(await response.Content.ReadAsByteArrayAsync()));
    }

    [Fact]
    public async Task PostArt_SuggestsADownloadFileName()
    {
        var response = await PostArtAsync("valorant-ace", "story");

        Assert.Equal("bora-live-valorant-ace-story.png", response.Content.Headers.ContentDisposition?.FileName);
    }

    [Fact]
    public async Task PostArt_LongTitleWithAccentsAndEmoji_DoesNotBreak()
    {
        var response = await PostArtAsync(liveTitle: "Só vitória hoje: ação, emoção e coração 🔥 até de madrugada");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostArt_UnknownTemplate_Returns404()
    {
        var response = await PostArtAsync(templateId: "nao-existe");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("art.template_not_found", await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task PostArt_UnknownFormat_Returns400()
    {
        var response = await PostArtAsync(format: "poster");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("art_format.unknown", await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task PostArt_TitleAboveTheLimit_Returns400WithDomainError()
    {
        var response = await PostArtAsync(liveTitle: new string('a', 61));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("art_request.live_title_too_long", await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task PostArt_LogoThatIsNotAnImage_Returns400()
    {
        var response = await PostArtAsync(logo: "isto não é uma imagem"u8.ToArray());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("art.logo_invalid", await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task PostArt_LogoAboveTheLimit_Returns413()
    {
        var response = await PostArtAsync(logo: new byte[ArtErrors.MaxLogoBytes + 1]);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal("art.logo_too_large", await ReadErrorCodeAsync(response));
    }

    // ---------- Fundo com IA ----------

    [Fact]
    public async Task PostArt_WithoutPrompt_NeverCallsGemini()
    {
        var response = await PostArtAsync(apiKey: SecretKey);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(WarningOf(response));
        Assert.Equal(0, _gemini.Calls);
    }

    [Fact]
    public async Task PostArt_WithPromptAndKey_UsesTheGeneratedBackground()
    {
        _gemini.ResponseJson = GeminiImageResponse();

        var response = await PostArtAsync(prompt: "pôr do sol neon", apiKey: SecretKey);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(WarningOf(response));
        Assert.Equal((1280, 720), ReadPngSize(await response.Content.ReadAsByteArrayAsync()));

        // A chamada ao Gemini: modelo de configuração, chave no header (nunca na URL), prompt e proporção.
        Assert.Equal(1, _gemini.Calls);
        Assert.Contains(":generateContent", _gemini.LastRequestUri!.AbsolutePath);
        Assert.Equal(SecretKey, _gemini.LastApiKeyHeader);
        Assert.DoesNotContain(SecretKey, _gemini.LastRequestUri.ToString());
        Assert.DoesNotContain(SecretKey, _gemini.LastRequestBody!);
        Assert.Contains("\"aspectRatio\":\"16:9\"", _gemini.LastRequestBody);
        Assert.Contains("pôr do sol neon", System.Text.RegularExpressions.Regex.Unescape(_gemini.LastRequestBody!));
    }

    [Fact]
    public async Task PostArt_BannerAsksGeminiForTheClosestAspectRatio()
    {
        _gemini.ResponseJson = GeminiImageResponse();

        await PostArtAsync("cs2-tatico", "banner", prompt: "neon", apiKey: SecretKey);

        Assert.Contains("\"aspectRatio\":\"21:9\"", _gemini.LastRequestBody);
    }

    [Fact]
    public async Task PostArt_PromptWithoutKey_ReturnsGradientArtWithMissingKeyWarning()
    {
        var response = await PostArtAsync(prompt: "neon");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("api_key.missing", WarningOf(response));
        Assert.Equal(0, _gemini.Calls);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, "api_key.invalid")]
    [InlineData(HttpStatusCode.TooManyRequests, "art.ai_unavailable")]
    [InlineData(HttpStatusCode.InternalServerError, "art.ai_unavailable")]
    [InlineData(HttpStatusCode.BadRequest, "art.ai_rejected")]
    public async Task PostArt_GeminiFails_ReturnsGradientArtWithTheWarning(HttpStatusCode status, string warning)
    {
        _gemini.StatusToReturn = status;

        var response = await PostArtAsync(prompt: "neon", apiKey: SecretKey);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(warning, WarningOf(response));
        Assert.Equal((1280, 720), ReadPngSize(await response.Content.ReadAsByteArrayAsync()));
    }

    [Fact]
    public async Task PostArt_GeminiRejectsTheKeyOnABadRequest_WarnsInvalidKey()
    {
        _gemini.StatusToReturn = HttpStatusCode.BadRequest;
        _gemini.ResponseJson = """{"error":{"status":"INVALID_ARGUMENT","details":[{"reason":"API_KEY_INVALID"}]}}""";

        var response = await PostArtAsync(prompt: "neon", apiKey: SecretKey);

        Assert.Equal("api_key.invalid", WarningOf(response));
    }

    [Fact]
    public async Task PostArt_GeminiAnswersWithoutImage_WarnsRejected()
    {
        // Resposta 200 sem imagem: é o que acontece quando o filtro de segurança do Gemini barra o pedido.
        _gemini.ResponseJson = """{"candidates":[{"finishReason":"IMAGE_SAFETY"}]}""";

        var response = await PostArtAsync(prompt: "neon", apiKey: SecretKey);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("art.ai_rejected", WarningOf(response));
    }

    [Fact]
    public async Task PostArt_GeminiNetworkFailure_WarnsUnavailable()
    {
        _gemini.ExceptionToThrow = new HttpRequestException("sem rede");

        var response = await PostArtAsync(prompt: "neon", apiKey: SecretKey);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("art.ai_unavailable", WarningOf(response));
    }

    [Fact]
    public async Task PostArt_GeminiReturnsSomethingThatIsNotAnImage_WarnsAndStillDeliversTheArt()
    {
        _gemini.ResponseJson =
            $$$"""{"candidates":[{"content":{"parts":[{"inlineData":{"data":"{{{Convert.ToBase64String("não sou imagem"u8)}}}"}}]}}]}""";

        var response = await PostArtAsync(prompt: "neon", apiKey: SecretKey);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("art.ai_unavailable", WarningOf(response));
        Assert.Equal((1280, 720), ReadPngSize(await response.Content.ReadAsByteArrayAsync()));
    }

    [Fact]
    public async Task PostArt_PromptAboveTheLimit_Returns400()
    {
        var response = await PostArtAsync(prompt: new string('a', 501), apiKey: SecretKey);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, _gemini.Calls);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task PostArt_NeverLeaksTheKeyInLogsOrResponse(HttpStatusCode geminiStatus)
    {
        _gemini.StatusToReturn = geminiStatus;
        _gemini.ResponseJson = GeminiImageResponse();

        var response = await PostArtAsync(prompt: "neon", apiKey: SecretKey);

        Assert.NotEmpty(factory.Logs.Messages);
        Assert.DoesNotContain(factory.Logs.Messages, message => message.Contains(SecretKey));
        Assert.DoesNotContain(SecretKey, response.Headers.ToString());
        Assert.DoesNotContain(SecretKey, response.Content.Headers.ToString());
    }

    [Fact]
    public async Task PostArt_CorsExposesTheWarningHeader()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/art")
        {
            Content = new MultipartFormDataContent
            {
                { new StringContent("cs2-tatico"), "templateId" },
                { new StringContent("thumbnail"), "format" },
                { new StringContent("mare_alta"), "streamerName" },
                { new StringContent("Live"), "liveTitle" },
                { new StringContent("neon"), "prompt" },
            },
        };
        request.Headers.Add("Origin", BoraLiveApiFactory.TestOrigin);

        var response = await _client.SendAsync(request);

        Assert.Contains("X-Bora-Warning", response.Headers.GetValues("Access-Control-Expose-Headers").Single());
    }
}
