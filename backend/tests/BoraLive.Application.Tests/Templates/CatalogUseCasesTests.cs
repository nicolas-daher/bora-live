using BoraLive.Application.Abstractions;
using BoraLive.Application.Formats;
using BoraLive.Application.Games;
using BoraLive.Application.Templates;
using BoraLive.Domain.Art;
using BoraLive.Domain.Games;
using BoraLive.Domain.Templates;
using NSubstitute;

namespace BoraLive.Application.Tests.Templates;

public class CatalogUseCasesTests
{
    private static readonly Game Cs2 = Game.Create("cs2", "Counter-Strike 2").Value;
    private static readonly Game Valorant = Game.Create("valorant", "Valorant").Value;

    private readonly ITemplateRepository _repository = Substitute.For<ITemplateRepository>();

    public CatalogUseCasesTests()
    {
        _repository.ListGamesAsync(Arg.Any<CancellationToken>()).Returns([Cs2, Valorant]);
    }

    private static GameTemplate Template(string id, Game game, params ArtFormat[] formats)
    {
        var layer = TextLayer.Create(TextRole.LiveTitle, 0.5, 0.5, 0.1, "#FFFFFF").Value;
        var background = TemplateBackground.Create(["#111111", "#EEEEEE"], 90).Value;
        var layouts = formats.Select(format => TemplateLayout.Create(format, [layer]).Value);

        return GameTemplate.Create(id, "Modelo de teste", game, "tô ao vivo", background, layouts).Value;
    }

    // ---------- ListGamesUseCase ----------

    [Fact]
    public async Task ListGames_ReturnsOneDtoPerGame()
    {
        var games = await new ListGamesUseCase(_repository).ExecuteAsync();

        Assert.Equal([new GameDto("cs2", "Counter-Strike 2"), new GameDto("valorant", "Valorant")], games);
    }

    // ---------- ListTemplatesUseCase ----------

    [Fact]
    public async Task ListTemplates_WithoutGame_AsksRepositoryForAll()
    {
        _repository.ListTemplatesAsync(null, Arg.Any<CancellationToken>())
            .Returns([Template("cs2-a", Cs2, ArtFormat.Thumbnail), Template("valorant-a", Valorant, ArtFormat.Story)]);

        var result = await new ListTemplatesUseCase(_repository).ExecuteAsync(gameId: null);

        Assert.True(result.IsSuccess);
        Assert.Equal(["cs2-a", "valorant-a"], result.Value.Select(template => template.Id));
    }

    [Fact]
    public async Task ListTemplates_MapsTemplateToDto()
    {
        _repository.ListTemplatesAsync("cs2", Arg.Any<CancellationToken>())
            .Returns([Template("cs2-a", Cs2, ArtFormat.Thumbnail, ArtFormat.Banner)]);

        var result = await new ListTemplatesUseCase(_repository).ExecuteAsync("cs2");

        var dto = Assert.Single(result.Value);
        Assert.Equal("cs2-a", dto.Id);
        Assert.Equal("Modelo de teste", dto.Name);
        Assert.Equal("cs2", dto.GameId);
        Assert.Equal(["thumbnail", "banner"], dto.Formats);
        Assert.Equal(["#111111", "#EEEEEE"], dto.Background.Colors);
        Assert.Equal(90, dto.Background.Angle);
    }

    [Theory]
    [InlineData("CS2")]
    [InlineData("  cs2  ")]
    public async Task ListTemplates_NormalizesTheGameId(string gameId)
    {
        _repository.ListTemplatesAsync("cs2", Arg.Any<CancellationToken>()).Returns([]);

        var result = await new ListTemplatesUseCase(_repository).ExecuteAsync(gameId);

        Assert.True(result.IsSuccess);
        await _repository.Received(1).ListTemplatesAsync("cs2", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListTemplates_UnknownGame_ReturnsGameNotFoundError()
    {
        var result = await new ListTemplatesUseCase(_repository).ExecuteAsync("fortnite");

        Assert.Equal(TemplateErrors.GameNotFound, result.Error);
        await _repository.DidNotReceiveWithAnyArgs().ListTemplatesAsync(default, default);
    }

    // ---------- ListFormatsUseCase ----------

    [Fact]
    public void ListFormats_ReturnsTheFourFormatsWithSizes()
    {
        var formats = new ListFormatsUseCase().Execute();

        Assert.Equal(4, formats.Count);
        Assert.Contains(new FormatDto("thumbnail", "Thumbnail", 1280, 720), formats);
        Assert.Contains(new FormatDto("story", "Story", 1080, 1920), formats);
    }
}
