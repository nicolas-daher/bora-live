using BoraLive.Domain.Art;

namespace BoraLive.Domain.Tests.Art;

public class ArtFormatTests
{
    [Theory]
    [InlineData("thumbnail", 1280, 720)]
    [InlineData("banner", 1200, 480)]
    [InlineData("post", 1080, 1080)]
    [InlineData("story", 1080, 1920)]
    public void FromKey_KnownKey_ReturnsFormatWithExpectedSize(string key, int width, int height)
    {
        var result = ArtFormat.FromKey(key);

        Assert.True(result.IsSuccess);
        Assert.Equal(key, result.Value.Key);
        Assert.Equal(width, result.Value.Width);
        Assert.Equal(height, result.Value.Height);
    }

    [Theory]
    [InlineData("THUMBNAIL")]
    [InlineData("  Story  ")]
    public void FromKey_IgnoresCaseAndSpaces(string key)
    {
        Assert.True(ArtFormat.FromKey(key).IsSuccess);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("poster")]
    public void FromKey_UnknownKey_ReturnsUnknownError(string? key)
    {
        var result = ArtFormat.FromKey(key);

        Assert.True(result.IsFailure);
        Assert.Equal(ArtFormatErrors.Unknown, result.Error);
    }

    [Fact]
    public void FromKey_ReturnsTheSameInstanceAsTheStaticField()
    {
        Assert.Same(ArtFormat.Thumbnail, ArtFormat.FromKey("thumbnail").Value);
    }

    [Fact]
    public void All_HasFourFormatsWithUniqueKeysAndValidSizes()
    {
        Assert.Equal(4, ArtFormat.All.Count);
        Assert.Equal(4, ArtFormat.All.Select(format => format.Key).Distinct().Count());
        Assert.All(ArtFormat.All, format =>
        {
            Assert.True(format.Width > 0);
            Assert.True(format.Height > 0);
        });
    }
}
