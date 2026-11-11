using BoraLive.Api.Http;
using BoraLive.Application.Formats;
using BoraLive.Application.Games;
using BoraLive.Application.Templates;

namespace BoraLive.Api.Endpoints;

/// <summary>Endpoints de consulta: o que o Bora Live tem para oferecer.</summary>
public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/games", ListGamesAsync)
            .WithTags("Catálogo")
            .WithSummary("Lista os jogos que têm modelos")
            .Produces<IReadOnlyList<GameDto>>();

        app.MapGet("/api/templates", ListTemplatesAsync)
            .WithTags("Catálogo")
            .WithSummary("Lista os modelos, de todos os jogos ou de um só (?game=cs2)")
            .Produces<IReadOnlyList<TemplateDto>>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet("/api/formats", ListFormats)
            .WithTags("Catálogo")
            .WithSummary("Lista os formatos de saída e as medidas de cada um")
            .Produces<IReadOnlyList<FormatDto>>();

        return app;
    }

    private static async Task<IResult> ListGamesAsync(ListGamesUseCase useCase, CancellationToken cancellationToken) =>
        Results.Ok(await useCase.ExecuteAsync(cancellationToken));

    // "game" vem da query string: /api/templates?game=cs2
    private static async Task<IResult> ListTemplatesAsync(
        string? game,
        ListTemplatesUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(game, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.Error!.ToProblem();
    }

    private static IResult ListFormats(ListFormatsUseCase useCase) =>
        Results.Ok(useCase.Execute());
}
