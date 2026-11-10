using BoraLive.Domain.Art;

namespace BoraLive.Domain.Tests.Art;

public class ArtRequestTests
{
    [Fact]
    public void Create_ValidData_ReturnsRequestWithTrimmedTexts()
    {
        var template = TestData.Template();

        var result = ArtRequest.Create(
            template,
            ArtFormat.Thumbnail,
            "  nicolas  ",
            "  Subindo de patente no CS2  ",
            "  pôr do sol neon  ");

        Assert.True(result.IsSuccess);
        Assert.Same(template, result.Value.Template);
        Assert.Same(ArtFormat.Thumbnail, result.Value.Format);
        Assert.Equal("nicolas", result.Value.StreamerName);
        Assert.Equal("Subindo de patente no CS2", result.Value.LiveTitle);
        Assert.Equal("pôr do sol neon", result.Value.AiPrompt);
    }

    [Fact]
    public void Create_FormatNotDeclaredByTemplate_ReturnsError()
    {
        var template = TestData.Template(ArtFormat.Thumbnail);

        var result = ArtRequest.Create(template, ArtFormat.Story, "nicolas", "Live de hoje");

        Assert.Equal(ArtRequestErrors.FormatNotSupported, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_MissingStreamerName_ReturnsError(string? streamerName)
    {
        var result = ArtRequest.Create(TestData.Template(), ArtFormat.Thumbnail, streamerName, "Live de hoje");

        Assert.Equal(ArtRequestErrors.StreamerNameRequired, result.Error);
    }

    [Fact]
    public void Create_StreamerNameAtTheLimit_Succeeds()
    {
        var name = new string('a', ArtRequest.MaxStreamerNameLength);

        var result = ArtRequest.Create(TestData.Template(), ArtFormat.Thumbnail, name, "Live de hoje");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_StreamerNameAboveTheLimit_ReturnsError()
    {
        var name = new string('a', ArtRequest.MaxStreamerNameLength + 1);

        var result = ArtRequest.Create(TestData.Template(), ArtFormat.Thumbnail, name, "Live de hoje");

        Assert.Equal(ArtRequestErrors.StreamerNameTooLong, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_MissingLiveTitle_ReturnsError(string? liveTitle)
    {
        var result = ArtRequest.Create(TestData.Template(), ArtFormat.Thumbnail, "nicolas", liveTitle);

        Assert.Equal(ArtRequestErrors.LiveTitleRequired, result.Error);
    }

    [Fact]
    public void Create_LiveTitleAboveTheLimit_ReturnsError()
    {
        var title = new string('a', ArtRequest.MaxLiveTitleLength + 1);

        var result = ArtRequest.Create(TestData.Template(), ArtFormat.Thumbnail, "nicolas", title);

        Assert.Equal(ArtRequestErrors.LiveTitleTooLong, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyAiPrompt_BecomesNull(string? aiPrompt)
    {
        var result = ArtRequest.Create(TestData.Template(), ArtFormat.Thumbnail, "nicolas", "Live de hoje", aiPrompt);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.AiPrompt);
    }

    [Fact]
    public void Create_AiPromptAboveTheLimit_ReturnsError()
    {
        var prompt = new string('a', ArtRequest.MaxAiPromptLength + 1);

        var result = ArtRequest.Create(TestData.Template(), ArtFormat.Thumbnail, "nicolas", "Live de hoje", prompt);

        Assert.Equal(ArtRequestErrors.AiPromptTooLong, result.Error);
    }

    [Fact]
    public void Create_NullTemplate_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ArtRequest.Create(null!, ArtFormat.Thumbnail, "nicolas", "Live de hoje"));
    }
}
