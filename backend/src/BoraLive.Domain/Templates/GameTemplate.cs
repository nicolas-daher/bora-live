using BoraLive.Domain.Art;
using BoraLive.Domain.Common;
using BoraLive.Domain.Games;

namespace BoraLive.Domain.Templates;

/// <summary>
/// Um modelo de arte de um jogo: o fundo, a chamada e um layout para cada formato que aceita.
/// </summary>
public sealed class GameTemplate
{
    public const int MaxNameLength = 60;
    public const int MaxCallToActionLength = 30;

    private GameTemplate(
        string id,
        string name,
        Game game,
        string callToAction,
        TemplateBackground background,
        IReadOnlyList<TemplateLayout> layouts)
    {
        Id = id;
        Name = name;
        Game = game;
        CallToAction = callToAction;
        Background = background;
        Layouts = layouts;
        Formats = layouts.Select(layout => layout.Format).ToArray();
    }

    /// <summary>Identificador em formato de slug (ex.: "cs2-neon").</summary>
    public string Id { get; }

    public string Name { get; }

    public Game Game { get; }

    /// <summary>Texto fixo da chamada para a live (ex.: "tô ao vivo").</summary>
    public string CallToAction { get; }

    public TemplateBackground Background { get; }

    public IReadOnlyList<TemplateLayout> Layouts { get; }

    /// <summary>Formatos que este modelo aceita: um para cada layout declarado.</summary>
    public IReadOnlyList<ArtFormat> Formats { get; }

    public static Result<GameTemplate> Create(
        string? id,
        string? name,
        Game game,
        string? callToAction,
        TemplateBackground background,
        IEnumerable<TemplateLayout> layouts)
    {
        // Argumento nulo aqui é erro de programação, não erro de negócio: por isso exceção.
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(background);
        ArgumentNullException.ThrowIfNull(layouts);

        if (!Slug.IsValid(id))
        {
            return GameTemplateErrors.InvalidId;
        }

        var trimmedName = name?.Trim();

        if (string.IsNullOrEmpty(trimmedName))
        {
            return GameTemplateErrors.NameRequired;
        }

        if (trimmedName.Length > MaxNameLength)
        {
            return GameTemplateErrors.NameTooLong;
        }

        var trimmedCallToAction = callToAction?.Trim();

        if (string.IsNullOrEmpty(trimmedCallToAction))
        {
            return GameTemplateErrors.CallToActionRequired;
        }

        if (trimmedCallToAction.Length > MaxCallToActionLength)
        {
            return GameTemplateErrors.CallToActionTooLong;
        }

        var layoutList = layouts.ToArray();

        if (layoutList.Length == 0)
        {
            return GameTemplateErrors.LayoutsRequired;
        }

        // Um layout por formato: dois layouts de thumbnail deixariam a escolha ambígua.
        if (layoutList.Select(layout => layout.Format).Distinct().Count() != layoutList.Length)
        {
            return GameTemplateErrors.DuplicateFormat;
        }

        return new GameTemplate(id!, trimmedName, game, trimmedCallToAction, background, layoutList);
    }

    /// <summary>Diz se este modelo aceita o formato informado.</summary>
    public bool Supports(ArtFormat format) => Formats.Contains(format);

    /// <summary>O layout deste modelo para o formato, ou null se ele não aceita o formato.</summary>
    public TemplateLayout? LayoutFor(ArtFormat format) =>
        Layouts.FirstOrDefault(layout => layout.Format == format);
}

public static class GameTemplateErrors
{
    public static readonly Error InvalidId = new(
        "game_template.invalid_id",
        "O identificador do modelo só pode ter letras minúsculas, números e hífen.");

    public static readonly Error NameRequired = new(
        "game_template.name_required",
        "Todo modelo precisa de um nome.");

    public static readonly Error NameTooLong = new(
        "game_template.name_too_long",
        $"O nome do modelo pode ter no máximo {GameTemplate.MaxNameLength} caracteres.");

    public static readonly Error CallToActionRequired = new(
        "game_template.call_to_action_required",
        "Todo modelo precisa de um texto de chamada.");

    public static readonly Error CallToActionTooLong = new(
        "game_template.call_to_action_too_long",
        $"A chamada do modelo pode ter no máximo {GameTemplate.MaxCallToActionLength} caracteres.");

    public static readonly Error LayoutsRequired = new(
        "game_template.layouts_required",
        "O modelo precisa de um layout para pelo menos um formato.");

    public static readonly Error DuplicateFormat = new(
        "game_template.duplicate_format",
        "O modelo está com dois layouts para o mesmo formato.");
}
