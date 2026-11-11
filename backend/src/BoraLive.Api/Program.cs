using BoraLive.Api.Endpoints;
using BoraLive.Api.Http;
using BoraLive.Application.ApiKeys;
using BoraLive.Application.Art;
using BoraLive.Application.Formats;
using BoraLive.Application.Games;
using BoraLive.Application.Templates;
using BoraLive.Infrastructure;

const string FrontendCorsPolicy = "frontend";

var builder = WebApplication.CreateBuilder(args);

// ---------- Serviços (injeção de dependência) ----------

// Health check nativo do ASP.NET Core: dispensa pacote extra.
builder.Services.AddHealthChecks();

// Gera o documento OpenAPI (a descrição da API em JSON).
builder.Services.AddOpenApi();

// Erros inesperados também saem no formato ProblemDetails.
builder.Services.AddProblemDetails();

// Infrastructure: registra o Gemini, os modelos em JSON e o compositor de imagem.
builder.Services.AddInfrastructure(builder.Configuration);

// Casos de uso da Application: classes simples, sem MediatR.
builder.Services.AddScoped<ValidateApiKeyUseCase>();
builder.Services.AddScoped<ListGamesUseCase>();
builder.Services.AddScoped<ListTemplatesUseCase>();
builder.Services.AddScoped<ListFormatsUseCase>();
builder.Services.AddScoped<GenerateArtUseCase>();

// CORS: só o endereço do frontend pode chamar a API pelo navegador.
// A lista vem da configuração (Cors:AllowedOrigins), nunca fixa no código.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .WithMethods("GET", "POST")
        .WithHeaders("Content-Type", ApiHeaders.GeminiApiKey)
        // Sem isto o navegador esconde o header do aviso do JavaScript.
        .WithExposedHeaders(ApiHeaders.BoraWarning));
});

var app = builder.Build();

// ---------- Pipeline (a ordem importa) ----------

// Converte exceções não tratadas em resposta 500 com ProblemDetails,
// sem vazar detalhes internos para quem chamou.
app.UseExceptionHandler();

app.UseCors(FrontendCorsPolicy);

// Documentação só em desenvolvimento: em produção ela não fica exposta.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // documento em /openapi/v1.json
    app.UseSwaggerUI(options =>
    {
        // A tela do Swagger (/swagger) lê o documento gerado acima.
        options.SwaggerEndpoint("/openapi/v1.json", "Bora Live API v1");
    });
}

app.MapHealthChecks("/health");
app.MapApiKeyEndpoints();
app.MapCatalogEndpoints();
app.MapArtEndpoints();

app.Run();
