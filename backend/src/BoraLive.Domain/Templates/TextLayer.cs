using BoraLive.Domain.Common;

namespace BoraLive.Domain.Templates;

/// <summary>
/// Uma camada de texto de um layout: o que mostrar, onde e com que aparência.
/// Posição e tamanhos são relativos (de 0 a 1) às medidas da arte.
/// </summary>
public sealed record TextLayer
{
    private TextLayer(
        TextRole role,
        double x,
        double y,
        double fontSize,
        string color,
        TextAlign align,
        TextFont font,
        double maxWidth)
    {
        Role = role;
        X = x;
        Y = y;
        FontSize = fontSize;
        Color = color;
        Align = align;
        Font = font;
        MaxWidth = maxWidth;
    }

    public TextRole Role { get; }

    /// <summary>
    /// Posição horizontal do ponto de ancoragem: 0 é a borda esquerda, 1 é a direita.
    /// O alinhamento diz se o texto começa, termina ou fica centrado nesse ponto.
    /// </summary>
    public double X { get; }

    /// <summary>Posição vertical do topo do texto: 0 é o topo da arte, 1 é a base.</summary>
    public double Y { get; }

    /// <summary>Tamanho da fonte como fração da altura da arte (ex.: 0.1 = 10% da altura).</summary>
    public double FontSize { get; }

    /// <summary>Cor em hexadecimal, no formato "#RRGGBB", sempre em maiúsculas.</summary>
    public string Color { get; }

    public TextAlign Align { get; }

    public TextFont Font { get; }

    /// <summary>
    /// Largura máxima do texto como fração da largura da arte.
    /// Passando disso, o texto quebra de linha ou encolhe.
    /// </summary>
    public double MaxWidth { get; }

    public static Result<TextLayer> Create(
        TextRole role,
        double x,
        double y,
        double fontSize,
        string? color,
        TextAlign align = TextAlign.Left,
        TextFont font = TextFont.Display,
        double maxWidth = 1)
    {
        if (!Enum.IsDefined(role))
        {
            return TextLayerErrors.InvalidRole;
        }

        if (!Enum.IsDefined(align) || !Enum.IsDefined(font))
        {
            return TextLayerErrors.InvalidStyle;
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

        if (!IsBetweenZeroAndOne(maxWidth) || maxWidth == 0)
        {
            return TextLayerErrors.MaxWidthOutOfRange;
        }

        var normalizedColor = HexColor.Normalize(color);

        if (normalizedColor is null)
        {
            return TextLayerErrors.InvalidColor;
        }

        return new TextLayer(role, x, y, fontSize, normalizedColor, align, font, maxWidth);
    }

    // A comparação "value >= 0 && value <= 1" já devolve false para NaN.
    private static bool IsBetweenZeroAndOne(double value) => value is >= 0 and <= 1;
}

/// <summary>Alinhamento horizontal do texto em relação ao ponto de ancoragem (X).</summary>
public enum TextAlign
{
    Left,
    Center,
    Right,
}

/// <summary>Qual fonte da identidade visual a camada usa.</summary>
public enum TextFont
{
    /// <summary>Unbounded: títulos.</summary>
    Display,

    /// <summary>Inter: textos de apoio.</summary>
    Body,
}

public static class TextLayerErrors
{
    public static readonly Error InvalidRole = new(
        "text_layer.invalid_role",
        "Essa camada de texto tem um papel que a gente não conhece.");

    public static readonly Error InvalidStyle = new(
        "text_layer.invalid_style",
        "Essa camada de texto tem um alinhamento ou uma fonte que a gente não conhece.");

    public static readonly Error PositionOutOfRange = new(
        "text_layer.position_out_of_range",
        "A posição do texto precisa ficar entre 0 e 1.");

    public static readonly Error FontSizeOutOfRange = new(
        "text_layer.font_size_out_of_range",
        "O tamanho do texto precisa ser maior que 0 e no máximo 1.");

    public static readonly Error MaxWidthOutOfRange = new(
        "text_layer.max_width_out_of_range",
        "A largura máxima do texto precisa ser maior que 0 e no máximo 1.");

    public static readonly Error InvalidColor = new(
        "text_layer.invalid_color",
        "A cor do texto precisa estar no formato #RRGGBB.");
}
