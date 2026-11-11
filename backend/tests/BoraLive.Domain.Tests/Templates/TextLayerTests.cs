using BoraLive.Domain.Templates;

namespace BoraLive.Domain.Tests.Templates;

public class TextLayerTests
{
    [Fact]
    public void Create_ValidData_ReturnsLayerWithUppercaseColor()
    {
        var result = TextLayer.Create(TextRole.LiveTitle, 0.1, 0.8, 0.12, " #ff4f8b ");

        Assert.True(result.IsSuccess);
        Assert.Equal(TextRole.LiveTitle, result.Value.Role);
        Assert.Equal(0.1, result.Value.X);
        Assert.Equal(0.8, result.Value.Y);
        Assert.Equal(0.12, result.Value.FontSize);
        Assert.Equal("#FF4F8B", result.Value.Color);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    public void Create_PositionAtTheEdges_Succeeds(double x, double y)
    {
        var result = TextLayer.Create(TextRole.StreamerName, x, y, 0.1, "#FFFFFF");

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(-0.01, 0.5)]
    [InlineData(1.01, 0.5)]
    [InlineData(0.5, -0.01)]
    [InlineData(0.5, 1.01)]
    [InlineData(double.NaN, 0.5)]
    public void Create_PositionOutOfRange_ReturnsError(double x, double y)
    {
        var result = TextLayer.Create(TextRole.StreamerName, x, y, 0.1, "#FFFFFF");

        Assert.Equal(TextLayerErrors.PositionOutOfRange, result.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(double.NaN)]
    public void Create_FontSizeOutOfRange_ReturnsError(double fontSize)
    {
        var result = TextLayer.Create(TextRole.StreamerName, 0.5, 0.5, fontSize, "#FFFFFF");

        Assert.Equal(TextLayerErrors.FontSizeOutOfRange, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("FFD166")]    // sem o #
    [InlineData("#FFF")]      // forma curta não é aceita
    [InlineData("#GGGGGG")]   // não é hexadecimal
    [InlineData("#FFD16600")] // com transparência
    public void Create_InvalidColor_ReturnsError(string? color)
    {
        var result = TextLayer.Create(TextRole.StreamerName, 0.5, 0.5, 0.1, color);

        Assert.Equal(TextLayerErrors.InvalidColor, result.Error);
    }

    [Fact]
    public void Create_UndefinedRole_ReturnsError()
    {
        var result = TextLayer.Create((TextRole)999, 0.5, 0.5, 0.1, "#FFFFFF");

        Assert.Equal(TextLayerErrors.InvalidRole, result.Error);
    }

    [Fact]
    public void Create_WithoutStyle_UsesLeftDisplayAndFullWidth()
    {
        var layer = TextLayer.Create(TextRole.StreamerName, 0.5, 0.5, 0.1, "#FFFFFF").Value;

        Assert.Equal(TextAlign.Left, layer.Align);
        Assert.Equal(TextFont.Display, layer.Font);
        Assert.Equal(1, layer.MaxWidth);
    }

    [Fact]
    public void Create_WithStyle_KeepsIt()
    {
        var layer = TextLayer.Create(
            TextRole.LiveTitle, 0.5, 0.5, 0.1, "#FFFFFF", TextAlign.Center, TextFont.Body, maxWidth: 0.8).Value;

        Assert.Equal(TextAlign.Center, layer.Align);
        Assert.Equal(TextFont.Body, layer.Font);
        Assert.Equal(0.8, layer.MaxWidth);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1.1)]
    [InlineData(double.NaN)]
    public void Create_MaxWidthOutOfRange_ReturnsError(double maxWidth)
    {
        var result = TextLayer.Create(TextRole.StreamerName, 0.5, 0.5, 0.1, "#FFFFFF", maxWidth: maxWidth);

        Assert.Equal(TextLayerErrors.MaxWidthOutOfRange, result.Error);
    }

    [Fact]
    public void Create_UndefinedAlignOrFont_ReturnsError()
    {
        var badAlign = TextLayer.Create(TextRole.StreamerName, 0.5, 0.5, 0.1, "#FFFFFF", align: (TextAlign)99);
        var badFont = TextLayer.Create(TextRole.StreamerName, 0.5, 0.5, 0.1, "#FFFFFF", font: (TextFont)99);

        Assert.Equal(TextLayerErrors.InvalidStyle, badAlign.Error);
        Assert.Equal(TextLayerErrors.InvalidStyle, badFont.Error);
    }
}
