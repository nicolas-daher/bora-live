var builder = WebApplication.CreateBuilder(args);

// Health check nativo do ASP.NET Core: dispensa pacote extra.
builder.Services.AddHealthChecks();

// Gera o documento OpenAPI (a descrição da API em JSON).
builder.Services.AddOpenApi();

var app = builder.Build();

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

app.Run();
