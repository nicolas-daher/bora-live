using BoraLive.Domain.Games;
using BoraLive.Domain.Templates;

namespace BoraLive.Application.Abstractions;

/// <summary>
/// Porta para buscar jogos e modelos.
/// A Application não sabe de onde eles vêm: hoje são arquivos JSON,
/// amanhã poderia ser um banco de dados, sem mudar nenhum caso de uso.
/// </summary>
public interface ITemplateRepository
{
    Task<IReadOnlyList<Game>> ListGamesAsync(CancellationToken cancellationToken);

    /// <summary>Lista os modelos. Com <paramref name="gameId"/>, só os daquele jogo.</summary>
    Task<IReadOnlyList<GameTemplate>> ListTemplatesAsync(string? gameId, CancellationToken cancellationToken);

    /// <summary>Busca um modelo pelo identificador. Devolve null se não existir.</summary>
    Task<GameTemplate?> FindTemplateAsync(string templateId, CancellationToken cancellationToken);
}
