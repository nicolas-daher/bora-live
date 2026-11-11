using BoraLive.Api.Http;
using BoraLive.Application.Art;
using Microsoft.AspNetCore.Mvc;

namespace BoraLive.Api.Endpoints;

public static class ArtEndpoints
{
    // Limite do corpo inteiro da requisição: o logo (até 2 MB) mais os campos de texto.
    // Barra uploads enormes antes de o servidor ler tudo para a memória.
    private const long MaxRequestBytes = ArtErrors.MaxLogoBytes + (256 * 1024);

    public static IEndpointRouteBuilder MapArtEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/art", GenerateAsync)
            .WithTags("Arte")
            .WithSummary("Gera a arte final em PNG a partir de um modelo")
            .WithMetadata(new RequestSizeLimitAttribute(MaxRequestBytes))
            // Formulários no ASP.NET exigem, por padrão, um token contra CSRF.
            // CSRF explora cookies de login, e esta API não tem login nem cookies:
            // a proteção não se aplica, então fica desligada neste endpoint.
            .DisableAntiforgery()
            .Produces(StatusCodes.Status200OK, contentType: "image/png")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge);

        return app;
    }

    // A requisição é multipart/form-data (formulário com anexo), por causa do logo.
    // [FromForm] lê cada campo do formulário; IFormFile é o arquivo enviado.
    private static async Task<IResult> GenerateAsync(
        [FromForm] string? templateId,
        [FromForm] string? format,
        [FromForm] string? streamerName,
        [FromForm] string? liveTitle,
        IFormFile? logo,
        GenerateArtUseCase useCase,
        CancellationToken cancellationToken)
    {
        // Confere o tamanho ANTES de copiar o arquivo para a memória.
        if (logo is { Length: > ArtErrors.MaxLogoBytes })
        {
            return ArtErrors.LogoTooLarge.ToProblem();
        }

        var input = new GenerateArtInput(
            templateId,
            format,
            streamerName,
            liveTitle,
            await ReadAllBytesAsync(logo, cancellationToken));

        var result = await useCase.ExecuteAsync(input, cancellationToken);

        return result.IsSuccess
            ? Results.File(result.Value.Png, "image/png", result.Value.FileName)
            : result.Error!.ToProblem();
    }

    private static async Task<byte[]?> ReadAllBytesAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        // O logo vive só nesta variável, durante a requisição. Nada é gravado em disco pelo nosso código.
        using var memory = new MemoryStream(capacity: (int)file.Length);
        await file.CopyToAsync(memory, cancellationToken);

        return memory.ToArray();
    }
}
