using BoraLive.Domain.Common;
using BoraLive.Domain.Templates;

namespace BoraLive.Domain.Art;

/// <summary>
/// O pedido de uma arte: qual modelo, em qual formato e com quais textos.
/// </summary>
public sealed class ArtRequest
{
    /// <summary>Mesmo limite do nome de usuário da Twitch.</summary>
    public const int MaxStreamerNameLength = 25;

    public const int MaxLiveTitleLength = 60;

    public const int MaxAiPromptLength = 500;

    private ArtRequest(
        GameTemplate template,
        ArtFormat format,
        string streamerName,
        string liveTitle,
        string? aiPrompt)
    {
        Template = template;
        Format = format;
        StreamerName = streamerName;
        LiveTitle = liveTitle;
        AiPrompt = aiPrompt;
    }

    public GameTemplate Template { get; }

    public ArtFormat Format { get; }

    public string StreamerName { get; }

    public string LiveTitle { get; }

    /// <summary>Prompt opcional para a IA gerar o fundo. Nulo quando a pessoa não informou.</summary>
    public string? AiPrompt { get; }

    public static Result<ArtRequest> Create(
        GameTemplate template,
        ArtFormat format,
        string? streamerName,
        string? liveTitle,
        string? aiPrompt = null)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(format);

        if (!template.Supports(format))
        {
            return ArtRequestErrors.FormatNotSupported;
        }

        var trimmedName = streamerName?.Trim();

        if (string.IsNullOrEmpty(trimmedName))
        {
            return ArtRequestErrors.StreamerNameRequired;
        }

        if (trimmedName.Length > MaxStreamerNameLength)
        {
            return ArtRequestErrors.StreamerNameTooLong;
        }

        var trimmedTitle = liveTitle?.Trim();

        if (string.IsNullOrEmpty(trimmedTitle))
        {
            return ArtRequestErrors.LiveTitleRequired;
        }

        if (trimmedTitle.Length > MaxLiveTitleLength)
        {
            return ArtRequestErrors.LiveTitleTooLong;
        }

        // Prompt vazio ou só com espaços conta como "não informado".
        var trimmedPrompt = string.IsNullOrWhiteSpace(aiPrompt) ? null : aiPrompt.Trim();

        if (trimmedPrompt?.Length > MaxAiPromptLength)
        {
            return ArtRequestErrors.AiPromptTooLong;
        }

        return new ArtRequest(template, format, trimmedName, trimmedTitle, trimmedPrompt);
    }
}

public static class ArtRequestErrors
{
    public static readonly Error FormatNotSupported = new(
        "art_request.format_not_supported",
        "Opa, esse modelo não tem esse formato. Escolhe outro formato ou outro modelo?");

    public static readonly Error StreamerNameRequired = new(
        "art_request.streamer_name_required",
        "Faltou o seu nome de streamer.");

    public static readonly Error StreamerNameTooLong = new(
        "art_request.streamer_name_too_long",
        $"O nome de streamer pode ter no máximo {ArtRequest.MaxStreamerNameLength} caracteres.");

    public static readonly Error LiveTitleRequired = new(
        "art_request.live_title_required",
        "Faltou o título da live.");

    public static readonly Error LiveTitleTooLong = new(
        "art_request.live_title_too_long",
        $"O título da live pode ter no máximo {ArtRequest.MaxLiveTitleLength} caracteres. Dá uma enxugada?");

    public static readonly Error AiPromptTooLong = new(
        "art_request.ai_prompt_too_long",
        $"O prompt da IA pode ter no máximo {ArtRequest.MaxAiPromptLength} caracteres.");
}
