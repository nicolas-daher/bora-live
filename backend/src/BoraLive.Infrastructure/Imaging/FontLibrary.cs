using BoraLive.Domain.Templates;
using SkiaSharp;

namespace BoraLive.Infrastructure.Imaging;

/// <summary>
/// As fontes da identidade visual, embutidas na DLL.
/// O container Linux não tem fonte instalada: sem estes arquivos, o texto sairia como quadradinhos.
/// </summary>
internal static class FontLibrary
{
    private const string ResourcePrefix = "BoraLive.Infrastructure.Imaging.Fonts.";

    // "wght" é o eixo de peso de uma fonte variável (400 = normal, 700 = negrito).
    // Uma fonte variável traz todos os pesos em um arquivo só; aqui escolhemos um.
    private const uint WeightAxis = ('w' << 24) | ('g' << 16) | ('h' << 8) | 't';

    // Lazy: cada fonte é carregada uma vez e reaproveitada em todas as artes.
    private static readonly Lazy<SKTypeface> Display = new(() => Load("Unbounded.ttf", weight: 700));
    private static readonly Lazy<SKTypeface> Body = new(() => Load("Inter.ttf", weight: 600));

    public static SKTypeface Get(TextFont font) => font switch
    {
        TextFont.Body => Body.Value,
        _ => Display.Value,
    };

    private static SKTypeface Load(string fileName, float weight)
    {
        using var stream = typeof(FontLibrary).Assembly.GetManifestResourceStream(ResourcePrefix + fileName)
            ?? throw new InvalidOperationException($"Fonte não encontrada na DLL: {fileName}.");

        using var data = SKData.Create(stream);

        var baseTypeface = SKTypeface.FromData(data)
            ?? throw new InvalidOperationException($"Não foi possível abrir a fonte {fileName}.");

        // Parte da posição atual em todos os eixos e troca só o peso.
        var position = baseTypeface.VariationDesignPosition;

        for (var i = 0; i < position.Length; i++)
        {
            if (position[i].Axis == WeightAxis)
            {
                position[i].Value = weight;
            }
        }

        return baseTypeface.Clone(position) ?? baseTypeface;
    }
}
