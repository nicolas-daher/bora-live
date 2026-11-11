using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using BoraLive.Application.Abstractions;
using BoraLive.Domain.Art;
using BoraLive.Domain.Common;
using BoraLive.Domain.Games;
using BoraLive.Domain.Templates;

namespace BoraLive.Infrastructure.Templates;

/// <summary>
/// Lê os jogos e os modelos de arquivos JSON embutidos na DLL (pasta Templates/Data).
/// Não há banco de dados no MVP: para criar um modelo, basta adicionar um arquivo JSON.
/// </summary>
public sealed class JsonTemplateRepository : ITemplateRepository
{
    private const string GamesResource = "BoraLive.Infrastructure.Templates.Data.games.json";
    private const string TemplatesPrefix = "BoraLive.Infrastructure.Templates.Data.templates.";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        // Lê "liveTitle" como TextRole.LiveTitle, "center" como TextAlign.Center etc.
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    // Lazy: os arquivos são lidos uma vez só, na primeira consulta, e ficam em memória.
    // Como a classe é registrada como singleton, todas as requisições reaproveitam.
    private readonly Lazy<Catalog> _catalog = new(LoadCatalog);

    public Task<IReadOnlyList<Game>> ListGamesAsync(CancellationToken cancellationToken) =>
        Task.FromResult(_catalog.Value.Games);

    public Task<IReadOnlyList<GameTemplate>> ListTemplatesAsync(string? gameId, CancellationToken cancellationToken)
    {
        var templates = _catalog.Value.Templates;

        IReadOnlyList<GameTemplate> result = gameId is null
            ? templates
            : templates.Where(template => template.Game.Id == gameId).ToArray();

        return Task.FromResult(result);
    }

    public Task<GameTemplate?> FindTemplateAsync(string templateId, CancellationToken cancellationToken) =>
        Task.FromResult(_catalog.Value.Templates.FirstOrDefault(template => template.Id == templateId));

    // ---------- Leitura e conversão ----------

    private sealed record Catalog(IReadOnlyList<Game> Games, IReadOnlyList<GameTemplate> Templates);

    private static Catalog LoadCatalog()
    {
        var assembly = typeof(JsonTemplateRepository).Assembly;

        var games = Read<GameJson[]>(assembly, GamesResource)
            .Select(json => Unwrap(Game.Create(json.Id, json.Name), GamesResource))
            .ToArray();

        var templates = assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(TemplatesPrefix, StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .Select(name => ToTemplate(Read<TemplateJson>(assembly, name), games, name))
            .ToArray();

        var duplicatedId = templates.GroupBy(template => template.Id).FirstOrDefault(group => group.Count() > 1);

        if (duplicatedId is not null)
        {
            throw new InvalidOperationException($"Há mais de um modelo com o id '{duplicatedId.Key}'.");
        }

        return new Catalog(games, templates);
    }

    private static T Read<T>(Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Arquivo de modelos não encontrado: {resourceName}.");

        return JsonSerializer.Deserialize<T>(stream, JsonOptions)
            ?? throw new InvalidOperationException($"Arquivo de modelos vazio: {resourceName}.");
    }

    private static GameTemplate ToTemplate(TemplateJson json, IReadOnlyList<Game> games, string source)
    {
        var game = games.FirstOrDefault(g => g.Id == json.GameId)
            ?? throw new InvalidOperationException($"O modelo em {source} aponta para um jogo que não existe: '{json.GameId}'.");

        var background = Unwrap(
            TemplateBackground.Create(json.Background?.Colors ?? [], json.Background?.Angle ?? 0),
            source);

        var layouts = (json.Layouts ?? []).Select(layout => ToLayout(layout, source)).ToArray();

        return Unwrap(
            GameTemplate.Create(json.Id, json.Name, game, json.CallToAction, background, layouts),
            source);
    }

    private static TemplateLayout ToLayout(LayoutJson json, string source)
    {
        var format = Unwrap(ArtFormat.FromKey(json.Format), source);

        var layers = (json.Layers ?? [])
            .Select(layer => Unwrap(
                TextLayer.Create(
                    layer.Role, layer.X, layer.Y, layer.FontSize, layer.Color, layer.Align, layer.Font, layer.MaxWidth),
                source))
            .ToArray();

        var logo = json.Logo is null
            ? null
            : Unwrap(LogoSlot.Create(json.Logo.X, json.Logo.Y, json.Logo.Size), source);

        return Unwrap(TemplateLayout.Create(format, layers, logo), source);
    }

    // Um JSON de modelo inválido é erro de quem escreveu o arquivo, não do usuário.
    // Por isso aqui a falha do Domain vira exceção, com o nome do arquivo na mensagem.
    private static T Unwrap<T>(Result<T> result, string source) =>
        result.IsSuccess
            ? result.Value
            : throw new InvalidOperationException(
                $"Modelo inválido em {source}: {result.Error!.Message} ({result.Error.Code})");

    // ---------- Formato dos arquivos JSON ----------
    // Classes só de leitura do arquivo. Elas não saem deste arquivo:
    // o resto do sistema só enxerga as entidades do Domain.

    private sealed record GameJson(string? Id, string? Name);

    private sealed record TemplateJson(
        string? Id,
        string? Name,
        string? GameId,
        string? CallToAction,
        BackgroundJson? Background,
        LayoutJson[]? Layouts);

    private sealed record BackgroundJson(string?[]? Colors, double Angle);

    private sealed record LayoutJson(string? Format, LogoJson? Logo, LayerJson[]? Layers);

    private sealed record LogoJson(double X, double Y, double Size);

    private sealed record LayerJson(
        TextRole Role,
        double X,
        double Y,
        double FontSize,
        string? Color,
        TextAlign Align = TextAlign.Left,
        TextFont Font = TextFont.Display,
        double MaxWidth = 1);
}
