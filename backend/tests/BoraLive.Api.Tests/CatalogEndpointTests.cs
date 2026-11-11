using System.Net;
using System.Net.Http.Json;
using BoraLive.Api.Tests.Support;
using BoraLive.Application.Formats;
using BoraLive.Application.Games;
using BoraLive.Application.Templates;

namespace BoraLive.Api.Tests;

// Estes testes leem os arquivos JSON de verdade.
// Se um modelo for escrito com um dado inválido, é aqui que o erro aparece.
public sealed class CatalogEndpointTests(BoraLiveApiFactory factory) : IClassFixture<BoraLiveApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetGames_ReturnsCs2AndValorant()
    {
        var games = await _client.GetFromJsonAsync<GameDto[]>("/api/games");

        Assert.Equal(["cs2", "valorant"], games!.Select(game => game.Id));
    }

    [Fact]
    public async Task GetTemplates_ReturnsEveryTemplateWithItsFormats()
    {
        var templates = await _client.GetFromJsonAsync<TemplateDto[]>("/api/templates");

        Assert.NotEmpty(templates!);
        Assert.All(templates!, template =>
        {
            Assert.NotEmpty(template.Formats);
            Assert.InRange(template.Background.Colors.Count, 2, 4);
        });
    }

    [Theory]
    [InlineData("cs2")]
    [InlineData("valorant")]
    public async Task GetTemplates_FilteredByGame_ReturnsOnlyThatGame(string gameId)
    {
        var templates = await _client.GetFromJsonAsync<TemplateDto[]>($"/api/templates?game={gameId}");

        Assert.NotEmpty(templates!);
        Assert.All(templates!, template => Assert.Equal(gameId, template.GameId));
    }

    [Fact]
    public async Task GetTemplates_EveryGameHasThumbnailAndBanner()
    {
        // Requisito do MVP: thumbnail e banner para cada jogo suportado.
        var games = await _client.GetFromJsonAsync<GameDto[]>("/api/games");

        foreach (var game in games!)
        {
            var templates = await _client.GetFromJsonAsync<TemplateDto[]>($"/api/templates?game={game.Id}");
            var formats = templates!.SelectMany(template => template.Formats).ToHashSet();

            Assert.Contains("thumbnail", formats);
            Assert.Contains("banner", formats);
        }
    }

    [Fact]
    public async Task GetTemplates_UnknownGame_Returns404Problem()
    {
        var response = await _client.GetAsync("/api/templates?game=fortnite");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetFormats_ReturnsTheFourFormats()
    {
        var formats = await _client.GetFromJsonAsync<FormatDto[]>("/api/formats");

        Assert.Equal(["thumbnail", "banner", "post", "story"], formats!.Select(format => format.Key));
    }
}
