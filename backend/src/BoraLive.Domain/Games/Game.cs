using BoraLive.Domain.Common;

namespace BoraLive.Domain.Games;

/// <summary>
/// Um jogo suportado pelo Bora Live (ex.: CS2, Valorant).
/// </summary>
public sealed record Game
{
    public const int MaxNameLength = 40;

    private Game(string id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>Identificador em formato de slug (ex.: "cs2").</summary>
    public string Id { get; }

    /// <summary>Nome de exibição (ex.: "Counter-Strike 2").</summary>
    public string Name { get; }

    public static Result<Game> Create(string? id, string? name)
    {
        if (!Slug.IsValid(id))
        {
            return GameErrors.InvalidId;
        }

        var trimmedName = name?.Trim();

        if (string.IsNullOrEmpty(trimmedName))
        {
            return GameErrors.NameRequired;
        }

        if (trimmedName.Length > MaxNameLength)
        {
            return GameErrors.NameTooLong;
        }

        return new Game(id!, trimmedName);
    }
}

public static class GameErrors
{
    public static readonly Error InvalidId = new(
        "game.invalid_id",
        "O identificador do jogo só pode ter letras minúsculas, números e hífen.");

    public static readonly Error NameRequired = new(
        "game.name_required",
        "Todo jogo precisa de um nome.");

    public static readonly Error NameTooLong = new(
        "game.name_too_long",
        $"O nome do jogo pode ter no máximo {Game.MaxNameLength} caracteres.");
}
