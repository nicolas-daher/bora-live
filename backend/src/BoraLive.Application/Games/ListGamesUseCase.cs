using BoraLive.Application.Abstractions;

namespace BoraLive.Application.Games;

/// <summary>O que a API devolve sobre um jogo.</summary>
public sealed record GameDto(string Id, string Name);

/// <summary>Caso de uso: listar os jogos que têm modelos.</summary>
public sealed class ListGamesUseCase(ITemplateRepository templates)
{
    public async Task<IReadOnlyList<GameDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var games = await templates.ListGamesAsync(cancellationToken);

        // Mapeamento manual e explícito: a entidade do Domain não sai da Application.
        return games
            .Select(game => new GameDto(game.Id, game.Name))
            .ToArray();
    }
}
