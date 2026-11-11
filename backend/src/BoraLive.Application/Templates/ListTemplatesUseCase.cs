using BoraLive.Application.Abstractions;
using BoraLive.Domain.Common;
using BoraLive.Domain.Templates;

namespace BoraLive.Application.Templates;

/// <summary>O fundo padrão de um modelo, para o frontend desenhar uma amostra.</summary>
public sealed record BackgroundDto(IReadOnlyList<string> Colors, double Angle);

/// <summary>O que a API devolve sobre um modelo.</summary>
/// <param name="Formats">Chaves dos formatos que o modelo aceita (ex.: "thumbnail").</param>
public sealed record TemplateDto(
    string Id,
    string Name,
    string GameId,
    IReadOnlyList<string> Formats,
    BackgroundDto Background);

public static class TemplateErrors
{
    public static readonly Error GameNotFound = new(
        "template.game_not_found",
        "Opa, esse jogo a gente ainda não tem. Escolhe um da lista?");
}

/// <summary>Caso de uso: listar os modelos, de todos os jogos ou de um só.</summary>
public sealed class ListTemplatesUseCase(ITemplateRepository templates)
{
    public async Task<Result<IReadOnlyList<TemplateDto>>> ExecuteAsync(
        string? gameId,
        CancellationToken cancellationToken = default)
    {
        var filter = string.IsNullOrWhiteSpace(gameId) ? null : gameId.Trim().ToLowerInvariant();

        if (filter is not null)
        {
            var games = await templates.ListGamesAsync(cancellationToken);

            // Jogo inexistente é erro, e não lista vazia:
            // assim um endereço digitado errado não passa por "jogo sem modelos".
            if (games.All(game => game.Id != filter))
            {
                return TemplateErrors.GameNotFound;
            }
        }

        var found = await templates.ListTemplatesAsync(filter, cancellationToken);

        return found.Select(ToDto).ToArray();
    }

    private static TemplateDto ToDto(GameTemplate template) => new(
        template.Id,
        template.Name,
        template.Game.Id,
        template.Formats.Select(format => format.Key).ToArray(),
        new BackgroundDto(template.Background.Colors, template.Background.Angle));
}
