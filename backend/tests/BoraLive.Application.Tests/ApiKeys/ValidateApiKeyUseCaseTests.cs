using BoraLive.Application.Abstractions;
using BoraLive.Application.ApiKeys;
using NSubstitute;

namespace BoraLive.Application.Tests.ApiKeys;

public class ValidateApiKeyUseCaseTests
{
    private const string SomeKey = "AIzaSy-chave_de_teste.123";

    // Substitute.For cria um objeto falso que implementa a interface.
    // O teste decide o que ele responde e depois confere como foi chamado.
    private readonly IAiImageGenerator _generator = Substitute.For<IAiImageGenerator>();

    private ValidateApiKeyUseCase CreateUseCase() => new(_generator);

    private void GeneratorAnswers(ApiKeyStatus status) =>
        _generator.ValidateKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(status);

    [Fact]
    public async Task Execute_KeyAcceptedByProvider_ReturnsSuccess()
    {
        GeneratorAnswers(ApiKeyStatus.Valid);

        var result = await CreateUseCase().ExecuteAsync(SomeKey);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Execute_KeyRejectedByProvider_ReturnsInvalidError()
    {
        GeneratorAnswers(ApiKeyStatus.Invalid);

        var result = await CreateUseCase().ExecuteAsync(SomeKey);

        Assert.Equal(ApiKeyErrors.Invalid, result.Error);
    }

    [Fact]
    public async Task Execute_ProviderUnavailable_ReturnsProviderUnavailableError()
    {
        GeneratorAnswers(ApiKeyStatus.Unavailable);

        var result = await CreateUseCase().ExecuteAsync(SomeKey);

        Assert.Equal(ApiKeyErrors.ProviderUnavailable, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Execute_MissingKey_ReturnsMissingErrorWithoutCallingProvider(string? apiKey)
    {
        var result = await CreateUseCase().ExecuteAsync(apiKey);

        Assert.Equal(ApiKeyErrors.Missing, result.Error);
        await _generator.DidNotReceiveWithAnyArgs().ValidateKeyAsync(default!, default);
    }

    [Theory]
    [InlineData("chave com espaço")]
    [InlineData("chave\r\nX-Outro-Header: valor")] // tentativa de injetar um header
    [InlineData("chavé")]
    public async Task Execute_KeyWithForbiddenCharacters_ReturnsInvalidErrorWithoutCallingProvider(string apiKey)
    {
        var result = await CreateUseCase().ExecuteAsync(apiKey);

        Assert.Equal(ApiKeyErrors.Invalid, result.Error);
        await _generator.DidNotReceiveWithAnyArgs().ValidateKeyAsync(default!, default);
    }

    [Fact]
    public async Task Execute_KeyAboveTheLimit_ReturnsInvalidErrorWithoutCallingProvider()
    {
        var apiKey = new string('a', ValidateApiKeyUseCase.MaxKeyLength + 1);

        var result = await CreateUseCase().ExecuteAsync(apiKey);

        Assert.Equal(ApiKeyErrors.Invalid, result.Error);
        await _generator.DidNotReceiveWithAnyArgs().ValidateKeyAsync(default!, default);
    }

    [Fact]
    public async Task Execute_TrimsTheKeyBeforeSendingToProvider()
    {
        GeneratorAnswers(ApiKeyStatus.Valid);

        await CreateUseCase().ExecuteAsync($"  {SomeKey}  ");

        await _generator.Received(1).ValidateKeyAsync(SomeKey, Arg.Any<CancellationToken>());
    }
}
