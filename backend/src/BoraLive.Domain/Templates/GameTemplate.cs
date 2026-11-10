using BoraLive.Domain.Art;
using BoraLive.Domain.Common;
using BoraLive.Domain.Games;

namespace BoraLive.Domain.Templates;

/// <summary>
/// Um modelo de arte de um jogo: quais formatos ele aceita e quais textos mostra.
/// </summary>
public sealed class GameTemplate
{
    public const int MaxNameLength = 60;

    private GameTemplate(
        string id,
        string name,
        Game game,
        IReadOnlyList<ArtFormat> formats,
        IReadOnlyList<TextLayer> layers)
    {
        Id = id;
        Name = name;
        Game = game;
        Formats = formats;
        Layers = layers;
    }

    /// <summary>Identificador em formato de slug (ex.: "cs2-neon").</summary>
    public string Id { get; }

    public string Name { get; }

    public Game Game { get; }

    /// <summary>Formatos que este modelo declara aceitar.</summary>
    public IReadOnlyList<ArtFormat> Formats { get; }

    public IReadOnlyList<TextLayer> Layers { get; }

    public static Result<GameTemplate> Create(
        string? id,
        string? name,
        Game game,
        IEnumerable<ArtFormat> formats,
        IEnumerable<TextLayer> layers)
    {
        // Argumento nulo aqui é erro de programação, não erro de negócio: por isso exceção.
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(formats);
        ArgumentNullException.ThrowIfNull(layers);

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

        // ToArray() cria uma cópia: quem chamou não consegue alterar a lista depois.
        var formatList = formats.ToArray();

        if (formatList.Length == 0)
        {
            return GameTemplateErrors.FormatsRequired;
        }

        if (formatList.Distinct().Count() != formatList.Length)
        {
            return GameTemplateErrors.DuplicateFormat;
        }

        var layerList = layers.ToArray();

        if (layerList.Length == 0)
        {
            return GameTemplateErrors.LayersRequired;
        }

        // Cada papel (nome, título, chamada) aparece no máximo uma vez por modelo.
        if (layerList.Select(layer => layer.Role).Distinct().Count() != layerList.Length)
        {
            return GameTemplateErrors.DuplicateLayerRole;
        }

        return new GameTemplate(id!, trimmedName, game, formatList, layerList);
    }

    /// <summary>Diz se este modelo aceita o formato informado.</summary>
    public bool Supports(ArtFormat format) => Formats.Contains(format);
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

    public static readonly Error FormatsRequired = new(
        "game_template.formats_required",
        "O modelo precisa aceitar pelo menos um formato.");

    public static readonly Error DuplicateFormat = new(
        "game_template.duplicate_format",
        "O modelo está com um formato repetido.");

    public static readonly Error LayersRequired = new(
        "game_template.layers_required",
        "O modelo precisa de pelo menos uma camada de texto.");

    public static readonly Error DuplicateLayerRole = new(
        "game_template.duplicate_layer_role",
        "O modelo está com duas camadas de texto para a mesma informação.");
}
