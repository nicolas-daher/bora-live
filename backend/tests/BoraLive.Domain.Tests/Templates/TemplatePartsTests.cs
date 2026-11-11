using BoraLive.Domain.Art;
using BoraLive.Domain.Templates;

namespace BoraLive.Domain.Tests.Templates;

public class TemplateLayoutTests
{
    [Fact]
    public void Create_ValidData_ReturnsLayout()
    {
        var logo = LogoSlot.Create(0.9, 0.1, 0.15).Value;

        var result = TemplateLayout.Create(ArtFormat.Story, [TestData.Layer()], logo);

        Assert.True(result.IsSuccess);
        Assert.Same(ArtFormat.Story, result.Value.Format);
        Assert.Single(result.Value.Layers);
        Assert.Equal(logo, result.Value.Logo);
    }

    [Fact]
    public void Create_WithoutLogo_LeavesLogoNull()
    {
        var layout = TemplateLayout.Create(ArtFormat.Story, [TestData.Layer()]).Value;

        Assert.Null(layout.Logo);
    }

    [Fact]
    public void Create_NoLayers_ReturnsError()
    {
        var result = TemplateLayout.Create(ArtFormat.Story, []);

        Assert.Equal(TemplateLayoutErrors.LayersRequired, result.Error);
    }

    [Fact]
    public void Create_TwoLayersWithSameRole_ReturnsError()
    {
        TextLayer[] layers = [TestData.Layer(TextRole.LiveTitle), TestData.Layer(TextRole.LiveTitle)];

        var result = TemplateLayout.Create(ArtFormat.Story, layers);

        Assert.Equal(TemplateLayoutErrors.DuplicateLayerRole, result.Error);
    }
}

public class TemplateBackgroundTests
{
    [Fact]
    public void Create_ValidData_ReturnsBackgroundWithUppercaseColors()
    {
        var result = TemplateBackground.Create([" #1a1033 ", "#ff4f8b"], 135);

        Assert.True(result.IsSuccess);
        Assert.Equal(["#1A1033", "#FF4F8B"], result.Value.Colors);
        Assert.Equal(135, result.Value.Angle);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void Create_ColorCountOutOfRange_ReturnsError(int count)
    {
        var colors = Enumerable.Repeat("#FFFFFF", count);

        var result = TemplateBackground.Create(colors, 0);

        Assert.Equal(TemplateBackgroundErrors.InvalidColorCount, result.Error);
    }

    [Fact]
    public void Create_InvalidColor_ReturnsError()
    {
        var result = TemplateBackground.Create(["#FFFFFF", "vermelho"], 0);

        Assert.Equal(TemplateBackgroundErrors.InvalidColor, result.Error);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(360)]
    [InlineData(double.NaN)]
    public void Create_AngleOutOfRange_ReturnsError(double angle)
    {
        var result = TemplateBackground.Create(["#FFFFFF", "#000000"], angle);

        Assert.Equal(TemplateBackgroundErrors.AngleOutOfRange, result.Error);
    }
}

public class LogoSlotTests
{
    [Fact]
    public void Create_ValidData_ReturnsSlot()
    {
        var result = LogoSlot.Create(0.9, 0.1, 0.15);

        Assert.True(result.IsSuccess);
        Assert.Equal(0.9, result.Value.X);
        Assert.Equal(0.1, result.Value.Y);
        Assert.Equal(0.15, result.Value.Size);
    }

    [Theory]
    [InlineData(-0.1, 0.5)]
    [InlineData(0.5, 1.1)]
    [InlineData(double.NaN, 0.5)]
    public void Create_PositionOutOfRange_ReturnsError(double x, double y)
    {
        Assert.Equal(LogoSlotErrors.PositionOutOfRange, LogoSlot.Create(x, y, 0.1).Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1.1)]
    [InlineData(double.NaN)]
    public void Create_SizeOutOfRange_ReturnsError(double size)
    {
        Assert.Equal(LogoSlotErrors.SizeOutOfRange, LogoSlot.Create(0.5, 0.5, size).Error);
    }
}
