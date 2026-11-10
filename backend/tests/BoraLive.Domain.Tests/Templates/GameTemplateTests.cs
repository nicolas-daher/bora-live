using BoraLive.Domain.Art;
using BoraLive.Domain.Templates;

namespace BoraLive.Domain.Tests.Templates;

public class GameTemplateTests
{
    private static readonly ArtFormat[] ValidFormats = [ArtFormat.Thumbnail, ArtFormat.Banner];

    private static TextLayer[] ValidLayers() =>
        [TestData.Layer(TextRole.StreamerName), TestData.Layer(TextRole.LiveTitle)];

    [Fact]
    public void Create_ValidData_ReturnsTemplate()
    {
        var game = TestData.Cs2();

        var result = GameTemplate.Create("cs2-neon", "  CS2 Neon  ", game, ValidFormats, ValidLayers());

        Assert.True(result.IsSuccess);
        Assert.Equal("cs2-neon", result.Value.Id);
        Assert.Equal("CS2 Neon", result.Value.Name);
        Assert.Equal(game, result.Value.Game);
        Assert.Equal(ValidFormats, result.Value.Formats);
        Assert.Equal(2, result.Value.Layers.Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("CS2 Neon")]
    public void Create_InvalidId_ReturnsError(string? id)
    {
        var result = GameTemplate.Create(id, "CS2 Neon", TestData.Cs2(), ValidFormats, ValidLayers());

        Assert.Equal(GameTemplateErrors.InvalidId, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_MissingName_ReturnsError(string? name)
    {
        var result = GameTemplate.Create("cs2-neon", name, TestData.Cs2(), ValidFormats, ValidLayers());

        Assert.Equal(GameTemplateErrors.NameRequired, result.Error);
    }

    [Fact]
    public void Create_NameAboveTheLimit_ReturnsError()
    {
        var name = new string('a', GameTemplate.MaxNameLength + 1);

        var result = GameTemplate.Create("cs2-neon", name, TestData.Cs2(), ValidFormats, ValidLayers());

        Assert.Equal(GameTemplateErrors.NameTooLong, result.Error);
    }

    [Fact]
    public void Create_NoFormats_ReturnsError()
    {
        var result = GameTemplate.Create("cs2-neon", "CS2 Neon", TestData.Cs2(), [], ValidLayers());

        Assert.Equal(GameTemplateErrors.FormatsRequired, result.Error);
    }

    [Fact]
    public void Create_DuplicateFormat_ReturnsError()
    {
        ArtFormat[] formats = [ArtFormat.Thumbnail, ArtFormat.Thumbnail];

        var result = GameTemplate.Create("cs2-neon", "CS2 Neon", TestData.Cs2(), formats, ValidLayers());

        Assert.Equal(GameTemplateErrors.DuplicateFormat, result.Error);
    }

    [Fact]
    public void Create_NoLayers_ReturnsError()
    {
        var result = GameTemplate.Create("cs2-neon", "CS2 Neon", TestData.Cs2(), ValidFormats, []);

        Assert.Equal(GameTemplateErrors.LayersRequired, result.Error);
    }

    [Fact]
    public void Create_TwoLayersWithSameRole_ReturnsError()
    {
        TextLayer[] layers = [TestData.Layer(TextRole.LiveTitle), TestData.Layer(TextRole.LiveTitle)];

        var result = GameTemplate.Create("cs2-neon", "CS2 Neon", TestData.Cs2(), ValidFormats, layers);

        Assert.Equal(GameTemplateErrors.DuplicateLayerRole, result.Error);
    }

    [Fact]
    public void Create_NullGame_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            GameTemplate.Create("cs2-neon", "CS2 Neon", null!, ValidFormats, ValidLayers()));
    }

    [Fact]
    public void Create_CopiesTheLists_SoLaterChangesDoNotAffectTheTemplate()
    {
        var formats = new List<ArtFormat> { ArtFormat.Thumbnail };

        var template = GameTemplate.Create("cs2-neon", "CS2 Neon", TestData.Cs2(), formats, ValidLayers()).Value;
        formats.Add(ArtFormat.Story);

        Assert.Single(template.Formats);
        Assert.False(template.Supports(ArtFormat.Story));
    }

    [Fact]
    public void Supports_ReturnsTrueOnlyForDeclaredFormats()
    {
        var template = TestData.Template(ArtFormat.Thumbnail, ArtFormat.Banner);

        Assert.True(template.Supports(ArtFormat.Thumbnail));
        Assert.True(template.Supports(ArtFormat.Banner));
        Assert.False(template.Supports(ArtFormat.Post));
        Assert.False(template.Supports(ArtFormat.Story));
    }
}
