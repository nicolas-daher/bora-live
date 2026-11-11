using BoraLive.Api.Http;
using BoraLive.Application.ApiKeys;
using Microsoft.AspNetCore.Mvc;

namespace BoraLive.Api.Endpoints;

public static class ApiKeyEndpoints
{
    public static IEndpointRouteBuilder MapApiKeyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/key").WithTags("Chave do Gemini");

        group.MapPost("/validate", ValidateAsync)
            .WithSummary("Testa a chave do Gemini enviada no header")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        return app;
    }

    // O ASP.NET preenche os parâmetros sozinho:
    // - apiKey vem do header indicado em [FromHeader];
    // - useCase vem da injeção de dependência;
    // - cancellationToken é cancelado se o navegador desistir da requisição.
    private static async Task<IResult> ValidateAsync(
        [FromHeader(Name = ApiHeaders.GeminiApiKey)] string? apiKey,
        ValidateApiKeyUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(apiKey, cancellationToken);

        // 204: deu certo e não há o que devolver. A chave nunca volta na resposta.
        return result.IsSuccess
            ? Results.NoContent()
            : result.Error!.ToProblem();
    }
}
