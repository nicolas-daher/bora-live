using BoraLive.Domain.Art;

namespace BoraLive.Application.Formats;

/// <summary>O que a API devolve sobre um formato de saída.</summary>
public sealed record FormatDto(string Key, string Name, int Width, int Height);

/// <summary>
/// Caso de uso: listar os formatos de saída.
/// Os formatos são fixos no Domain, então aqui não há porta nem acesso externo.
/// </summary>
public sealed class ListFormatsUseCase
{
    public IReadOnlyList<FormatDto> Execute() =>
        ArtFormat.All
            .Select(format => new FormatDto(format.Key, format.Name, format.Width, format.Height))
            .ToArray();
}
