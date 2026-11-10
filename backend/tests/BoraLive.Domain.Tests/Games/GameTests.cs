using BoraLive.Domain.Games;

namespace BoraLive.Domain.Tests.Games;

public class GameTests
{
    [Fact]
    public void Create_ValidData_ReturnsGameWithTrimmedName()
    {
        var result = Game.Create("cs2", "  Counter-Strike 2  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("cs2", result.Value.Id);
        Assert.Equal("Counter-Strike 2", result.Value.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("CS2")]          // maiúsculas
    [InlineData("counter strike")] // espaço
    [InlineData("cs_2")]         // caractere fora da regra
    [InlineData("-cs2")]         // começa com hífen
    [InlineData("cs2-")]         // termina com hífen
    public void Create_InvalidId_ReturnsInvalidIdError(string? id)
    {
        var result = Game.Create(id, "Counter-Strike 2");

        Assert.Equal(GameErrors.InvalidId, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_MissingName_ReturnsNameRequiredError(string? name)
    {
        var result = Game.Create("cs2", name);

        Assert.Equal(GameErrors.NameRequired, result.Error);
    }

    [Fact]
    public void Create_NameAtTheLimit_Succeeds()
    {
        var result = Game.Create("cs2", new string('a', Game.MaxNameLength));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_NameAboveTheLimit_ReturnsNameTooLongError()
    {
        var result = Game.Create("cs2", new string('a', Game.MaxNameLength + 1));

        Assert.Equal(GameErrors.NameTooLong, result.Error);
    }

    [Fact]
    public void Games_WithSameData_AreEqual()
    {
        var first = Game.Create("cs2", "Counter-Strike 2").Value;
        var second = Game.Create("cs2", "Counter-Strike 2").Value;

        Assert.Equal(first, second);
    }
}
