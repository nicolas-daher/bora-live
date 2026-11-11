namespace BoraLive.Domain.Common;

/// <summary>
/// Regra das cores do domínio: sempre "#RRGGBB", guardadas em maiúsculas.
/// </summary>
internal static class HexColor
{
    /// <summary>Devolve a cor aparada e em maiúsculas, ou null se não estiver no formato.</summary>
    public static string? Normalize(string? value)
    {
        var color = value?.Trim().ToUpperInvariant();

        var isValid = color is { Length: 7 }
            && color[0] == '#'
            && color.Skip(1).All(char.IsAsciiHexDigit);

        return isValid ? color : null;
    }
}
