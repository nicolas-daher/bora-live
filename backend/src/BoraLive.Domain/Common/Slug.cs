namespace BoraLive.Domain.Common;

/// <summary>
/// Regra dos identificadores de texto (ex.: "cs2", "valorant-neon"):
/// letras minúsculas, números e hífen, sem começar nem terminar com hífen.
/// </summary>
internal static class Slug
{
    public const int MaxLength = 50;

    public static bool IsValid(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > MaxLength)
        {
            return false;
        }

        if (value[0] == '-' || value[^1] == '-')
        {
            return false;
        }

        return value.All(c => c is (>= 'a' and <= 'z') or (>= '0' and <= '9') or '-');
    }
}
