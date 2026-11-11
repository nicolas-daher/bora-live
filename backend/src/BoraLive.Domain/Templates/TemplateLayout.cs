using BoraLive.Domain.Art;
using BoraLive.Domain.Common;

namespace BoraLive.Domain.Templates;

/// <summary>
/// A disposição de um modelo em UM formato: onde vai cada texto e o logo.
/// Cada formato tem o seu layout, porque um banner e um story têm proporções opostas.
/// </summary>
public sealed class TemplateLayout
{
    private TemplateLayout(ArtFormat format, IReadOnlyList<TextLayer> layers, LogoSlot? logo)
    {
        Format = format;
        Layers = layers;
        Logo = logo;
    }

    public ArtFormat Format { get; }

    public IReadOnlyList<TextLayer> Layers { get; }

    /// <summary>Lugar do logo do streamer. Nulo quando o layout não mostra logo.</summary>
    public LogoSlot? Logo { get; }

    public static Result<TemplateLayout> Create(ArtFormat format, IEnumerable<TextLayer> layers, LogoSlot? logo = null)
    {
        ArgumentNullException.ThrowIfNull(format);
        ArgumentNullException.ThrowIfNull(layers);

        // ToArray() cria uma cópia: quem chamou não consegue alterar a lista depois.
        var layerList = layers.ToArray();

        if (layerList.Length == 0)
        {
            return TemplateLayoutErrors.LayersRequired;
        }

        // Cada papel (nome, título, chamada) aparece no máximo uma vez por layout.
        if (layerList.Select(layer => layer.Role).Distinct().Count() != layerList.Length)
        {
            return TemplateLayoutErrors.DuplicateLayerRole;
        }

        return new TemplateLayout(format, layerList, logo);
    }
}

public static class TemplateLayoutErrors
{
    public static readonly Error LayersRequired = new(
        "template_layout.layers_required",
        "O layout precisa de pelo menos uma camada de texto.");

    public static readonly Error DuplicateLayerRole = new(
        "template_layout.duplicate_layer_role",
        "O layout está com duas camadas de texto para a mesma informação.");
}
