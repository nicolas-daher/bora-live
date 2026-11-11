using BoraLive.Domain.Art;
using BoraLive.Domain.Common;
using BoraLive.Domain.Templates;

namespace BoraLive.Domain.Tests.Templates;

public class GameTemplateTests
{
    private static TemplateLayout[] ValidLayouts() =>
        [TestData.Layout(ArtFormat.Thumbnail), TestData.Layout(ArtFormat.Banner)];

    // Monta um modelo válido, trocando só o que o teste quer testar.
    private static Result<GameTemplate> Create(
        string? id = "cs2-neon",
        string? name = "CS2 Neon",
        string? callToAction = "tô ao vivo",
        IEnumerable<TemplateLayout>? layouts = null) =>
        GameTemplate.Create(id, name, TestData.Cs2(), callToAction, TestData.Background(), layouts ?? ValidLayouts());

    [Fact]
    public void Create_ValidData_ReturnsTemplate()
    {
        var result = Create(name: "  CS2 Neon  ", callToAction: "  tô ao vivo  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("cs2-neon", result.Value.Id);
        Assert.Equal("CS2 Neon", result.Value.Name);
        Assert.Equal("tô ao vivo", result.Value.CallToAction);
        Assert.Equal(TestData.Cs2(), result.Value.Game);
        Assert.Equal(2, result.Value.Layouts.Count);
    }

    [Fact]
    public void Create_DerivesFormatsFromLayouts()
    {
        var template = Create().Value;

        Assert.Equal([ArtFormat.Thumbnail, ArtFormat.Banner], template.Formats);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("CS2 Neon")]
    public void Create_InvalidId_ReturnsError(string? id)
    {
        Assert.Equal(GameTemplateErrors.InvalidId, Create(id: id).Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_MissingName_ReturnsError(string? name)
    {
        Assert.Equal(GameTemplateErrors.NameRequired, Create(name: name).Error);
    }

    [Fact]
    public void Create_NameAboveTheLimit_ReturnsError()
    {
        var name = new string('a', GameTemplate.MaxNameLength + 1);

        Assert.Equal(GameTemplateErrors.NameTooLong, Create(name: name).Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_MissingCallToAction_ReturnsError(string? callToAction)
    {
        Assert.Equal(GameTemplateErrors.CallToActionRequired, Create(callToAction: callToAction).Error);
    }

    [Fact]
    public void Create_CallToActionAboveTheLimit_ReturnsError()
    {
        var callToAction = new string('a', GameTemplate.MaxCallToActionLength + 1);

        Assert.Equal(GameTemplateErrors.CallToActionTooLong, Create(callToAction: callToAction).Error);
    }

    [Fact]
    public void Create_NoLayouts_ReturnsError()
    {
        Assert.Equal(GameTemplateErrors.LayoutsRequired, Create(layouts: []).Error);
    }

    [Fact]
    public void Create_TwoLayoutsForTheSameFormat_ReturnsError()
    {
        TemplateLayout[] layouts = [TestData.Layout(ArtFormat.Thumbnail), TestData.Layout(ArtFormat.Thumbnail)];

        Assert.Equal(GameTemplateErrors.DuplicateFormat, Create(layouts: layouts).Error);
    }

    [Fact]
    public void Create_NullGame_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            GameTemplate.Create("cs2-neon", "CS2 Neon", null!, "tô ao vivo", TestData.Background(), ValidLayouts()));
    }

    [Fact]
    public void Create_CopiesTheList_SoLaterChangesDoNotAffectTheTemplate()
    {
        var layouts = new List<TemplateLayout> { TestData.Layout(ArtFormat.Thumbnail) };

        var template = Create(layouts: layouts).Value;
        layouts.Add(TestData.Layout(ArtFormat.Story));

        Assert.Single(template.Layouts);
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

    [Fact]
    public void LayoutFor_ReturnsTheLayoutOfTheFormat_OrNull()
    {
        var template = TestData.Template(ArtFormat.Thumbnail);

        Assert.Same(ArtFormat.Thumbnail, template.LayoutFor(ArtFormat.Thumbnail)!.Format);
        Assert.Null(template.LayoutFor(ArtFormat.Story));
    }
}
