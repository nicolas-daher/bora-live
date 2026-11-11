using System.Collections.Concurrent;
using System.Net;
using BoraLive.Application.Abstractions;
using BoraLive.Infrastructure.Gemini;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BoraLive.Api.Tests.Support;

/// <summary>
/// Sobe a API inteira em memória, com duas trocas:
/// 1. O Gemini é simulado: nenhuma requisição sai para a internet.
/// 2. Tudo o que a API escreve em log é capturado, para os testes inspecionarem.
///
/// A troca é feita na "ponta" do HttpClient (o handler que faria a chamada de rede).
/// Assim o GeminiImageGenerator de verdade e o log do HttpClient continuam rodando,
/// e o teste de segurança da chave vale para o código real.
/// </summary>
public sealed class BoraLiveApiFactory : WebApplicationFactory<Program>
{
    public const string TestOrigin = "http://localhost:5173";

    public FakeGeminiHandler Gemini { get; } = new();

    public CapturingLoggerProvider Logs { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("Cors:AllowedOrigins:0", TestOrigin);

        builder.ConfigureLogging(logging =>
        {
            // Nível mais detalhado possível: se a chave vazar em qualquer nível, o teste pega.
            logging.SetMinimumLevel(LogLevel.Trace);
            logging.AddFilter(_ => true);
            logging.AddProvider(Logs);
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddHttpClient<IAiImageGenerator, GeminiImageGenerator>()
                .ConfigurePrimaryHttpMessageHandler(() => Gemini);
        });
    }
}

/// <summary>Faz o papel do Gemini: guarda a requisição recebida e devolve o status combinado.</summary>
public sealed class FakeGeminiHandler : HttpMessageHandler
{
    public HttpStatusCode StatusToReturn { get; set; } = HttpStatusCode.OK;

    /// <summary>Quando preenchida, simula uma falha de rede.</summary>
    public Exception? ExceptionToThrow { get; set; }

    public Uri? LastRequestUri { get; private set; }

    public string? LastApiKeyHeader { get; private set; }

    public int Calls { get; private set; }

    /// <summary>Corpo JSON da resposta. Nulo = resposta sem corpo (basta para validar a chave).</summary>
    public string? ResponseJson { get; set; }

    /// <summary>Corpo da última requisição recebida (o pedido de geração de imagem).</summary>
    public string? LastRequestBody { get; private set; }

    /// <summary>Volta ao estado inicial, para um teste não herdar a configuração do anterior.</summary>
    public void Reset()
    {
        StatusToReturn = HttpStatusCode.OK;
        ExceptionToThrow = null;
        ResponseJson = null;
        LastRequestBody = null;
        LastRequestUri = null;
        LastApiKeyHeader = null;
        Calls = 0;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Calls++;
        LastRequestUri = request.RequestUri;
        LastApiKeyHeader = request.Headers.TryGetValues(GeminiImageGenerator.ApiKeyHeader, out var values)
            ? string.Join(",", values)
            : null;
        LastRequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        var response = new HttpResponseMessage(StatusToReturn);

        if (ResponseJson is not null)
        {
            response.Content = new StringContent(ResponseJson, System.Text.Encoding.UTF8, "application/json");
        }

        return response;
    }

    // O HttpClientFactory descarta os handlers de tempos em tempos.
    // Este é compartilhado pela fábrica de testes, então ignora o descarte.
    protected override void Dispose(bool disposing)
    {
    }
}

/// <summary>Um destino de log que só guarda as mensagens em memória.</summary>
public sealed class CapturingLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentQueue<string> _messages = new();

    public IReadOnlyCollection<string> Messages => _messages;

    public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, _messages);

    public void Dispose()
    {
    }

    private sealed class CapturingLogger(string category, ConcurrentQueue<string> messages) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            // Os "escopos" também carregam dados para o log: entram na captura.
            messages.Enqueue($"[scope] {category}: {state}");
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            messages.Enqueue($"[{logLevel}] {category}: {formatter(state, exception)} {exception}");

            // Além do texto final, guarda cada campo estruturado da mensagem.
            if (state is IEnumerable<KeyValuePair<string, object?>> fields)
            {
                foreach (var field in fields)
                {
                    messages.Enqueue($"[field] {field.Key}={field.Value}");
                }
            }
        }
    }
}
