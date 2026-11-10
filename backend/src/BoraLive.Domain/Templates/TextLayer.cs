using BoraLive.Domain.Common;

namespace BoraLive.Domain.Templates;

/// <summary>
/// Uma camada de texto de um modelo: o que mostrar, onde e com que tamanho e cor.
/// Posição e tamanho são relativos (de 0 a 1), para a mesma camada servir
/// em qualquer formato sem refazer contas em pixels.
/// </summary>
public sealed record TextLayer
{
    private TextLayer(TextRole role, double x, double y, double fontSize, string color)
    {
        Role = role;
        X = x;
        Y = y;
        FontSize = fontSize;
        Color = color;
    }

    public TextRole Role { get; }

    /// <summary>Posição horizontal: 0 é a borda esquerda, 1 é a direita.</summary>
    public double X { get; }

    /// <summary>Posição vertical: 0 é o topo, 1 é a base.</summary>
    public double Y { get; }

    /// <summary>Tamanho da fonte como fração da altura da arte (ex.: 0.1 = 10% da altura).</summary>
    public double FontSize { get; }

    /// <summary>Cor em hexadecimal, no formato "#RRGGBB", sempre em maiúsculas.</summary>
    public string Color { get; }

    public static Result<TextLayer> Create(TextRole role, double x, double y, double fontSize, string? color)
    {
        if (!Enum.IsDefined(role))
        {
            return TextLayerErrors.InvalidRole;
        }

        if (!IsBetweenZeroAndOne(x) || !IsBetweenZeroAndOne(y))
        {
            return TextLayerErrors.PositionOutOfRange;
        }

        // O tamanho precisa ser maior que zero: texto de altura zero não aparece.
        if (!IsBetweenZeroAndOne(fontSize) || fontSize == 0)
        {
            return TextLayerErrors.FontSizeOutOfRange;
        }

        var normalizedColor = color?.Trim().ToUpperInvariant();

        if (!IsHexColor(normalizedColor))
        {
            return TextLayerErrors.InvalidColor;
        }

        return new TextLayer(role, x, y, fontSize, normalizedColor!);
    }

    // A comparação "value >= 0 && value <= 1" já devolve false para NaN.
    private static bool IsBetweenZeroAndOne(double value) => value is >= 0 and <= 1;

    private static bool IsHexColor(string? value) =>
        value is { Length: 7 }
        && value[0] == '#'
        && value.Skip(1).All(char.IsAsciiHexDigit);
}

public static class TextLayerErrors
{
    public static readonly Error InvalidRole = new(
        "text_layer.invalid_role",
        "Essa camada de texto tem um papel que a gente não conhece.");

    public static readonly Error PositionOutOfRange = new(
        "text_layer.position_out_of_range",
        "A posição do texto precisa ficar entre 0 e 1.");

    public static readonly Error FontSizeOutOfRange = new(
        "text_layer.font_size_out_of_range",
        "O tamanho do texto precisa ser maior que 0 e no máximo 1.");

    public static readonly Error InvalidColor = new(
        "text_layer.invalid_color",
        "A cor do texto precisa estar no formato #RRGGBB.");
}
