using System.Net;
using System.Text.Json;
using BoraLive.Api.Http;
using BoraLive.Api.Tests.Support;

namespace BoraLive.Api.Tests;

// Cada teste cria a sua própria fábrica: assim um não enxerga os logs
// nem as chamadas ao Gemini simulado do outro.
public sealed class ApiKeyEndpointTests : IDisposable
{
    // Valor fácil de procurar nos logs e que não se parece com nenhum outro texto.
    private const string SecretKey = "AIzaSy-CHAVE_SECRETA_DE_TESTE.9f3b7c";

    private readonly BoraLiveApiFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    private Task<HttpResponseMessage> PostValidateAsync(string? apiKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/key/validate");

        if (apiKey is not null)
        {
            request.Headers.Add(ApiHeaders.GeminiApiKey, apiKey);
        }

        return _factory.CreateClient().SendAsync(request);
    }

    private static async Task<string?> ReadErrorCodeAsync(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("code").GetString();
    }

    [Fact]
    public async Task Validate_KeyAcceptedByGemini_Returns204WithEmptyBody()
    {
        _factory.Gemini.StatusToReturn = HttpStatusCode.OK;

        var response = await PostValidateAsync(SecretKey);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Validate_SendsKeyToGeminiInHeaderAndNeverInUrl()
    {
        await PostValidateAsync(SecretKey);

        Assert.Equal(SecretKey, _factory.Gemini.LastApiKeyHeader);
        Assert.DoesNotContain(SecretKey, _factory.Gemini.LastRequestUri!.ToString());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task Validate_KeyRejectedByGemini_Returns401Problem(HttpStatusCode geminiStatus)
    {
        _factory.Gemini.StatusToReturn = geminiStatus;

        var response = await PostValidateAsync(SecretKey);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("api_key.invalid", await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task Validate_MissingHeader_Returns400ProblemWithoutCallingGemini()
    {
        var response = await PostValidateAsync(apiKey: null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("api_key.missing", await ReadErrorCodeAsync(response));
        Assert.Equal(0, _factory.Gemini.Calls);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task Validate_GeminiWithProblems_Returns503Problem(HttpStatusCode geminiStatus)
    {
        _factory.Gemini.StatusToReturn = geminiStatus;

        var response = await PostValidateAsync(SecretKey);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("api_key.provider_unavailable", await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task Validate_NetworkFailure_Returns503Problem()
    {
        _factory.Gemini.ExceptionToThrow = new HttpRequestException("sem conexão");

        var response = await PostValidateAsync(SecretKey);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    // ---------- Segurança da chave (regra inegociável do projeto) ----------

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task Validate_NeverWritesTheKeyToLogs(HttpStatusCode geminiStatus)
    {
        _factory.Gemini.StatusToReturn = geminiStatus;

        await PostValidateAsync(SecretKey);

        // Garante que o teste não passa "por sorte": a chamada ao Gemini precisa
        // estar nos logs capturados (o HttpClient registra o endereço que chamou).
        Assert.Contains(_factory.Logs.Messages, message => message.Contains("v1beta/models"));
        Assert.DoesNotContain(_factory.Logs.Messages, message => message.Contains(SecretKey));
    }

    [Fact]
    public async Task Validate_NetworkFailure_NeverWritesTheKeyToLogs()
    {
        _factory.Gemini.ExceptionToThrow = new HttpRequestException("sem conexão");

        await PostValidateAsync(SecretKey);

        Assert.NotEmpty(_factory.Logs.Messages);
        Assert.DoesNotContain(_factory.Logs.Messages, message => message.Contains(SecretKey));
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task Validate_NeverReturnsTheKeyInTheResponse(HttpStatusCode geminiStatus)
    {
        _factory.Gemini.StatusToReturn = geminiStatus;

        var response = await PostValidateAsync(SecretKey);

        Assert.DoesNotContain(SecretKey, await response.Content.ReadAsStringAsync());
        Assert.DoesNotContain(SecretKey, response.Headers.ToString());
    }

    // ---------- CORS ----------

    [Theory]
    [InlineData(BoraLiveApiFactory.TestOrigin, true)]
    [InlineData("https://site-malicioso.example", false)]
    public async Task Preflight_AllowsOnlyTheFrontendOrigin(string origin, bool shouldBeAllowed)
    {
        // "Preflight": a pergunta que o navegador faz antes de mandar um header fora do padrão.
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/key/validate");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", ApiHeaders.GeminiApiKey);

        var response = await _factory.CreateClient().SendAsync(request);

        Assert.Equal(shouldBeAllowed, response.Headers.Contains("Access-Control-Allow-Origin"));
    }
}
