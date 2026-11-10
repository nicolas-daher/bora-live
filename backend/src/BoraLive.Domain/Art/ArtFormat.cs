using BoraLive.Domain.Common;

namespace BoraLive.Domain.Art;

/// <summary>
/// Um formato de saída da arte, com as medidas em pixels.
/// O conjunto é fechado: só existem as instâncias declaradas aqui.
/// </summary>
public sealed class ArtFormat
{
    public static readonly ArtFormat Thumbnail = new("thumbnail", "Thumbnail", 1280, 720);
    public static readonly ArtFormat Banner = new("banner", "Banner de perfil", 1200, 480);
    public static readonly ArtFormat Post = new("post", "Post quadrado", 1080, 1080);
    public static readonly ArtFormat Story = new("story", "Story", 1080, 1920);

    /// <summary>Todos os formatos que o Bora Live gera.</summary>
    public static IReadOnlyList<ArtFormat> All { get; } = [Thumbnail, Banner, Post, Story];

    // Construtor privado: ninguém de fora cria um formato com medidas inventadas.
    private ArtFormat(string key, string name, int width, int height)
    {
        Key = key;
        Name = name;
        Width = width;
        Height = height;
    }

    /// <summary>Identificador usado no JSON e na API (ex.: "thumbnail").</summary>
    public string Key { get; }

    /// <summary>Nome de exibição.</summary>
    public string Name { get; }

    public int Width { get; }

    public int Height { get; }

    /// <summary>Busca um formato pela chave, sem diferenciar maiúsculas de minúsculas.</summary>
    public static Result<ArtFormat> FromKey(string? key)
    {
        var format = All.FirstOrDefault(f =>
            string.Equals(f.Key, key?.Trim(), StringComparison.OrdinalIgnoreCase));

        return format is null
            ? ArtFormatErrors.Unknown
            : format;
    }

    public override string ToString() => $"{Key} ({Width}x{Height})";
}

public static class ArtFormatErrors
{
    public static readonly Error Unknown = new(
        "art_format.unknown",
        "Opa, esse formato a gente não conhece. Escolhe um dos formatos da lista?");
}
