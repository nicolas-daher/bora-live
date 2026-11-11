using BoraLive.Application.Abstractions;
using BoraLive.Application.ApiKeys;
using BoraLive.Domain.Art;
using BoraLive.Domain.Common;

namespace BoraLive.Application.Art;

/// <summary>O que a pessoa informou para gerar uma arte.</summary>
/// <param name="Logo">Bytes do logo do streamer, ou null quando não foi enviado.</param>
/// <param name="AiPrompt">O que a pessoa quer no fundo. Sem prompt, a IA nem é chamada.</param>
/// <param name="ApiKey">A chave do Gemini do usuário. Só passa por aqui: não é guardada.</param>
public sealed record GenerateArtInput(
    string? TemplateId,
    string? Format,
    string? StreamerName,
    string? LiveTitle,
    byte[]? Logo = null,
    string? AiPrompt = null,
    string? ApiKey = null);

/// <summary>A arte pronta.</summary>
/// <param name="Png">O arquivo PNG.</param>
/// <param name="FileName">Nome sugerido para o download.</param>
/// <param name="AiWarning">
/// Preenchido quando a pessoa pediu fundo com IA mas não deu certo: a arte saiu com o degradê do modelo.
/// </param>
public sealed record GeneratedArt(byte[] Png, string FileName, Error? AiWarning = null);

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

    // Os dois erros abaixo viram avisos: a arte sai com o degradê e a pessoa é informada.
    public static readonly Error AiUnavailable = new(
        "art.ai_unavailable",
        "A IA não respondeu agora. Tenta de novo daqui a pouco?");

    public static readonly Error AiRejected = new(
        "art.ai_rejected",
        "A IA não topou gerar esse fundo. Tenta descrever de outro jeito?");
}

/// <summary>
/// Caso de uso: gerar a arte final em PNG a partir de um modelo.
/// O logo é usado só na composição e descartado: nada é salvo.
/// </summary>
public sealed class GenerateArtUseCase(
    ITemplateRepository templates,
    IImageComposer composer,
    IAiImageGenerator aiImageGenerator)
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
        var request = ArtRequest.Create(
            template, format.Value, input.StreamerName, input.LiveTitle, input.AiPrompt);

        if (request.IsFailure)
        {
            return request.Error!;
        }

        // 5. Fundo com IA (opcional). Se não rolar, a arte sai com o degradê e a pessoa é avisada:
        //    perder a arte inteira por causa de um enfeite seria pior que um fundo mais simples.
        var (background, aiWarning) = await TryGenerateBackgroundAsync(
            request.Value, input.ApiKey, cancellationToken);

        // 6. Desenha.
        var logo = input.Logo is { Length: > 0 } ? input.Logo : null;
        var png = composer.Compose(request.Value, logo, background);

        // O fundo da IA veio, mas não dá para abrir (arquivo corrompido ou formato estranho):
        // desenha de novo só com o degradê.
        if (png.IsFailure && background is not null && png.Error == ArtErrors.AiUnavailable)
        {
            aiWarning = ArtErrors.AiUnavailable;
            png = composer.Compose(request.Value, logo, null);
        }

        if (png.IsFailure)
        {
            return png.Error!;
        }

        return new GeneratedArt(png.Value, $"bora-live-{template.Id}-{format.Value.Key}.png", aiWarning);
    }

    private async Task<(byte[]? Background, Error? Warning)> TryGenerateBackgroundAsync(
        ArtRequest request,
        string? apiKey,
        CancellationToken cancellationToken)
    {
        // Sem prompt, a pessoa não pediu IA: segue o baile com o degradê, sem aviso.
        if (request.AiPrompt is null)
        {
            return (null, null);
        }

        var key = apiKey?.Trim();

        if (string.IsNullOrEmpty(key))
        {
            return (null, ApiKeyErrors.Missing);
        }

        // Chave com caractere estranho nunca é válida e não deve chegar ao header da chamada.
        if (!ValidateApiKeyUseCase.IsWellFormed(key))
        {
            return (null, ApiKeyErrors.Invalid);
        }

        var result = await aiImageGenerator.GenerateBackgroundAsync(
            key,
            BuildBackgroundPrompt(request),
            request.Format.Width,
            request.Format.Height,
            cancellationToken);

        if (result.IsFailure)
        {
            return (null, result.Error);
        }

        return (result.Value, null);
    }

    /// <summary>
    /// Monta o pedido para a IA. As regras fixas ficam aqui, e não na tela, para valerem sempre:
    /// sem texto na imagem (o texto é desenhado por nós) e sem copiar arte oficial do jogo.
    /// </summary>
    private static string BuildBackgroundPrompt(ArtRequest request) =>
        $"Create a background image for a live stream graphic, in the general visual style of the video game "
        + $"\"{request.Template.Game.Name}\". Vibrant, cinematic, neon sunset lighting. "
        + "Keep the center calm and uncluttered so text can be placed over it. "
        + "Do NOT include any text, letters, numbers, logos, watermarks, or UI. "
        + "Do NOT reproduce official characters, maps, logos or artwork from the game; create original imagery only. "
        + $"Theme requested by the streamer: {request.AiPrompt}";
}
