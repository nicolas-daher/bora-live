using BoraLive.Application.Abstractions;
using BoraLive.Domain.Art;
using BoraLive.Domain.Common;

namespace BoraLive.Application.Art;

/// <summary>O que a pessoa informou para gerar uma arte.</summary>
/// <param name="Logo">Bytes do logo do streamer, ou null quando não foi enviado.</param>
public sealed record GenerateArtInput(
    string? TemplateId,
    string? Format,
    string? StreamerName,
    string? LiveTitle,
    byte[]? Logo = null);

/// <summary>A arte pronta.</summary>
/// <param name="Png">O arquivo PNG.</param>
/// <param name="FileName">Nome sugerido para o download.</param>
public sealed record GeneratedArt(byte[] Png, string FileName);

public static class ArtErrors
{
    /// <summary>Tamanho máximo do logo enviado: 2 MB.</summary>
    public const int MaxLogoBytes = 2 * 1024 * 1024;

    public static readonly Error TemplateNotFound = new(
        "art.template_not_found",
        "Opa, esse modelo a gente não achou. Escolhe um da lista?");

    public static readonly Error LogoTooLarge = new(
        "art.logo_too_large",
        "Esse logo tá pesado demais. Manda um de até 2 MB?");

    public static readonly Error LogoInvalid = new(
        "art.logo_invalid",
        "Não deu pra abrir esse logo. Manda um PNG, JPG ou WebP?");
}

/// <summary>
/// Caso de uso: gerar a arte final em PNG a partir de um modelo.
/// O logo é usado só na composição e descartado: nada é salvo.
/// </summary>
public sealed class GenerateArtUseCase(ITemplateRepository templates, IImageComposer composer)
{
    public async Task<Result<GeneratedArt>> ExecuteAsync(
        GenerateArtInput input,
        CancellationToken cancellationToken = default)
    {
        // 1. O modelo existe?
        var templateId = input.TemplateId?.Trim().ToLowerInvariant();

        var template = string.IsNullOrEmpty(templateId)
            ? null
            : await templates.FindTemplateAsync(templateId, cancellationToken);

        if (template is null)
        {
            return ArtErrors.TemplateNotFound;
        }

        // 2. O formato existe?
        var format = ArtFormat.FromKey(input.Format);

        if (format.IsFailure)
        {
            return format.Error!;
        }

        // 3. O logo cabe no limite? (Conferido antes de qualquer trabalho pesado.)
        if (input.Logo is { Length: > ArtErrors.MaxLogoBytes })
        {
            return ArtErrors.LogoTooLarge;
        }

        // 4. As regras do pedido: nome, título, modelo aceita o formato. Quem valida é o Domain.
        var request = ArtRequest.Create(template, format.Value, input.StreamerName, input.LiveTitle);

        if (request.IsFailure)
        {
            return request.Error!;
        }

        // 5. Desenha.
        var logo = input.Logo is { Length: > 0 } ? input.Logo : null;
        var png = composer.Compose(request.Value, logo);

        if (png.IsFailure)
        {
            return png.Error!;
        }

        return new GeneratedArt(png.Value, $"bora-live-{template.Id}-{format.Value.Key}.png");
    }
}
