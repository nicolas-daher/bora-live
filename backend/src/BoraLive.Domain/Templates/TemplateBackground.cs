using BoraLive.Domain.Common;

namespace BoraLive.Domain.Templates;

/// <summary>
/// O fundo padrão de um modelo: um degradê linear.
/// É o fundo usado quando a pessoa não gera um com IA.
/// </summary>
public sealed class TemplateBackground
{
    public const int MinColors = 2;
    public const int MaxColors = 4;

    private TemplateBackground(IReadOnlyList<string> colors, double angle)
    {
        Colors = colors;
        Angle = angle;
    }

    /// <summary>Cores do degradê, na ordem, em "#RRGGBB".</summary>
    public IReadOnlyList<string> Colors { get; }

    /// <summary>
    /// Direção do degradê em graus, no mesmo sentido do CSS:
    /// 0 sobe, 90 vai para a direita, 180 desce.
    /// </summary>
    public double Angle { get; }

    public static Result<TemplateBackground> Create(IEnumerable<string?> colors, double angle)
    {
        ArgumentNullException.ThrowIfNull(colors);

        var normalized = colors.Select(HexColor.Normalize).ToArray();

        if (normalized.Length is < MinColors or > MaxColors)
        {
            return TemplateBackgroundErrors.InvalidColorCount;
        }

        if (normalized.Any(color => color is null))
        {
            return TemplateBackgroundErrors.InvalidColor;
        }

        if (angle is not (>= 0 and < 360))
        {
            return TemplateBackgroundErrors.AngleOutOfRange;
        }

        return new TemplateBackground(normalized!, angle);
    }
}

public static class TemplateBackgroundErrors
{
    public static readonly Error InvalidColorCount = new(
        "template_background.invalid_color_count",
        $"O degradê do fundo precisa ter de {TemplateBackground.MinColors} a {TemplateBackground.MaxColors} cores.");

    public static readonly Error InvalidColor = new(
        "template_background.invalid_color",
        "As cores do fundo precisam estar no formato #RRGGBB.");

    public static readonly Error AngleOutOfRange = new(
        "template_background.angle_out_of_range",
        "O ângulo do degradê precisa ser de 0 até menos de 360 graus.");
}
