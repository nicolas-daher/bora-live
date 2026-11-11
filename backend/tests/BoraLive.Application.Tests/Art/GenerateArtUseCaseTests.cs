using BoraLive.Application.Abstractions;
using BoraLive.Application.ApiKeys;
using BoraLive.Application.Art;
using BoraLive.Domain.Art;
using BoraLive.Domain.Common;
using BoraLive.Domain.Games;
using BoraLive.Domain.Templates;
using NSubstitute;

namespace BoraLive.Application.Tests.Art;

public class GenerateArtUseCaseTests
{
    private static readonly byte[] FakePng = [1, 2, 3];

    private readonly ITemplateRepository _repository = Substitute.For<ITemplateRepository>();
    private static readonly byte[] FakeBackground = [4, 5, 6];

    private readonly IImageComposer _composer = Substitute.For<IImageComposer>();
    private readonly IAiImageGenerator _ai = Substitute.For<IAiImageGenerator>();

    public GenerateArtUseCaseTests()
    {
        // Cenário padrão: existe o modelo "cs2-teste", só com thumbnail, e o compositor funciona.
        _repository.FindTemplateAsync("cs2-teste", Arg.Any<CancellationToken>()).Returns(Template());
        _composer.Compose(Arg.Any<ArtRequest>(), Arg.Any<byte[]?>(), Arg.Any<byte[]?>())
            .Returns(Result<byte[]>.Success(FakePng));
        _ai.GenerateBackgroundAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<byte[]>.Success(FakeBackground));
    }

    private static GameTemplate Template()
    {
        var game = Game.Create("cs2", "Counter-Strike 2").Value;
        var layer = TextLayer.Create(TextRole.LiveTitle, 0.5, 0.5, 0.1, "#FFFFFF").Value;
        var background = TemplateBackground.Create(["#111111", "#EEEEEE"], 90).Value;
        var layout = TemplateLayout.Create(ArtFormat.Thumbnail, [layer]).Value;

        return GameTemplate.Create("cs2-teste", "Teste", game, "tô ao vivo", background, [layout]).Value;
    }

    private static GenerateArtInput ValidInput(byte[]? logo = null) =>
        new("cs2-teste", "thumbnail", "mare_alta", "Subindo de elo", logo);

    private Task<Result<GeneratedArt>> ExecuteAsync(GenerateArtInput input) =>
        new GenerateArtUseCase(_repository, _composer, _ai).ExecuteAsync(input);

    [Fact]
    public async Task Execute_ValidInput_ReturnsPngAndFileName()
    {
        var result = await ExecuteAsync(ValidInput());

        Assert.True(result.IsSuccess);
        Assert.Equal(FakePng, result.Value.Png);
        Assert.Equal("bora-live-cs2-teste-thumbnail.png", result.Value.FileName);
    }

    [Fact]
    public async Task Execute_SendsTheValidatedRequestToTheComposer()
    {
        await ExecuteAsync(ValidInput() with { StreamerName = "  mare_alta  " });

        // Arg.Is confere o argumento que o compositor recebeu.
        _composer.Received(1).Compose(
            Arg.Is<ArtRequest>(request =>
                request.StreamerName == "mare_alta"
                && request.LiveTitle == "Subindo de elo"
                && request.Format == ArtFormat.Thumbnail
                && request.Template.Id == "cs2-teste"),
            null,
            null);
    }

    [Fact]
    public async Task Execute_WithLogo_PassesTheLogoToTheComposer()
    {
        byte[] logo = [9, 8, 7];

        await ExecuteAsync(ValidInput(logo));

        _composer.Received(1).Compose(Arg.Any<ArtRequest>(), logo, null);
    }

    [Fact]
    public async Task Execute_EmptyLogo_IsTreatedAsNoLogo()
    {
        await ExecuteAsync(ValidInput(logo: []));

        _composer.Received(1).Compose(Arg.Any<ArtRequest>(), null, null);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("nao-existe")]
    public async Task Execute_UnknownTemplate_ReturnsTemplateNotFound(string? templateId)
    {
        var result = await ExecuteAsync(ValidInput() with { TemplateId = templateId });

        Assert.Equal(ArtErrors.TemplateNotFound, result.Error);
        AssertComposerWasNotCalled();
    }

    [Fact]
    public async Task Execute_UnknownFormat_ReturnsFormatError()
    {
        var result = await ExecuteAsync(ValidInput() with { Format = "poster" });

        Assert.Equal(ArtFormatErrors.Unknown, result.Error);
        AssertComposerWasNotCalled();
    }

    [Fact]
    public async Task Execute_FormatNotSupportedByTemplate_ReturnsDomainError()
    {
        var result = await ExecuteAsync(ValidInput() with { Format = "story" });

        Assert.Equal(ArtRequestErrors.FormatNotSupported, result.Error);
        AssertComposerWasNotCalled();
    }

    [Fact]
    public async Task Execute_MissingStreamerName_ReturnsDomainError()
    {
        var result = await ExecuteAsync(ValidInput() with { StreamerName = " " });

        Assert.Equal(ArtRequestErrors.StreamerNameRequired, result.Error);
        AssertComposerWasNotCalled();
    }

    [Fact]
    public async Task Execute_LogoAboveTheLimit_ReturnsLogoTooLarge()
    {
        var logo = new byte[ArtErrors.MaxLogoBytes + 1];

        var result = await ExecuteAsync(ValidInput(logo));

        Assert.Equal(ArtErrors.LogoTooLarge, result.Error);
        AssertComposerWasNotCalled();
    }

    [Fact]
    public async Task Execute_ComposerRejectsTheLogo_ReturnsTheComposerError()
    {
        _composer.Compose(Arg.Any<ArtRequest>(), Arg.Any<byte[]?>(), Arg.Any<byte[]?>())
            .Returns(Result<byte[]>.Failure(ArtErrors.LogoInvalid));

        var result = await ExecuteAsync(ValidInput(logo: [1, 2, 3]));

        Assert.Equal(ArtErrors.LogoInvalid, result.Error);
    }

    // ---------- Fundo com IA ----------

    [Fact]
    public async Task Execute_WithoutPrompt_NeverCallsTheAi()
    {
        var result = await ExecuteAsync(ValidInput() with { ApiKey = "chave-valida" });

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.AiWarning);
        await _ai.DidNotReceiveWithAnyArgs()
            .GenerateBackgroundAsync(default!, default!, default, default, default);
    }

    [Fact]
    public async Task Execute_WithPromptAndKey_SendsTheGeneratedBackgroundToTheComposer()
    {
        var result = await ExecuteAsync(ValidInput() with { AiPrompt = "pôr do sol neon", ApiKey = "chave-valida" });

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.AiWarning);
        _composer.Received(1).Compose(Arg.Any<ArtRequest>(), null, FakeBackground);
    }

    [Fact]
    public async Task Execute_WithPrompt_BuildsAPromptWithGameStyleAndNoTextRules()
    {
        await ExecuteAsync(ValidInput() with { AiPrompt = "pôr do sol neon", ApiKey = "chave-valida" });

        await _ai.Received(1).GenerateBackgroundAsync(
            "chave-valida",
            Arg.Is<string>(prompt =>
                prompt.Contains("Counter-Strike 2")
                && prompt.Contains("pôr do sol neon")
                && prompt.Contains("Do NOT include any text")),
            1280,
            720,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_PromptWithoutKey_FallsBackToGradientWithMissingKeyWarning()
    {
        var result = await ExecuteAsync(ValidInput() with { AiPrompt = "neon", ApiKey = "  " });

        Assert.True(result.IsSuccess);
        Assert.Equal(ApiKeyErrors.Missing, result.Value.AiWarning);
        _composer.Received(1).Compose(Arg.Any<ArtRequest>(), null, null);
        await _ai.DidNotReceiveWithAnyArgs()
            .GenerateBackgroundAsync(default!, default!, default, default, default);
    }

    [Fact]
    public async Task Execute_PromptWithMalformedKey_DoesNotSendItToTheAi()
    {
        var result = await ExecuteAsync(ValidInput() with { AiPrompt = "neon", ApiKey = "chave com espaço\r\n" });

        Assert.Equal(ApiKeyErrors.Invalid, result.Value.AiWarning);
        await _ai.DidNotReceiveWithAnyArgs()
            .GenerateBackgroundAsync(default!, default!, default, default, default);
    }

    [Theory]
    [MemberData(nameof(AiFailures))]
    public async Task Execute_AiFails_FallsBackToGradientAndReturnsTheWarning(Error failure)
    {
        _ai.GenerateBackgroundAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<byte[]>.Failure(failure));

        var result = await ExecuteAsync(ValidInput() with { AiPrompt = "neon", ApiKey = "chave-valida" });

        Assert.True(result.IsSuccess);
        Assert.Equal(failure, result.Value.AiWarning);
        Assert.Equal(FakePng, result.Value.Png);
        _composer.Received(1).Compose(Arg.Any<ArtRequest>(), null, null);
    }

    public static TheoryData<Error> AiFailures() =>
        new() { ApiKeyErrors.Invalid, ArtErrors.AiUnavailable, ArtErrors.AiRejected };

    [Fact]
    public async Task Execute_ComposerCannotOpenTheBackground_RedrawsWithGradientAndWarns()
    {
        _composer.Compose(Arg.Any<ArtRequest>(), Arg.Any<byte[]?>(), FakeBackground)
            .Returns(Result<byte[]>.Failure(ArtErrors.AiUnavailable));

        var result = await ExecuteAsync(ValidInput() with { AiPrompt = "neon", ApiKey = "chave-valida" });

        Assert.True(result.IsSuccess);
        Assert.Equal(ArtErrors.AiUnavailable, result.Value.AiWarning);
        _composer.Received(1).Compose(Arg.Any<ArtRequest>(), null, null);
    }

    [Fact]
    public async Task Execute_PromptTooLong_ReturnsDomainErrorBeforeCallingTheAi()
    {
        var prompt = new string('a', ArtRequest.MaxAiPromptLength + 1);

        var result = await ExecuteAsync(ValidInput() with { AiPrompt = prompt, ApiKey = "chave-valida" });

        Assert.Equal(ArtRequestErrors.AiPromptTooLong, result.Error);
        await _ai.DidNotReceiveWithAnyArgs()
            .GenerateBackgroundAsync(default!, default!, default, default, default);
    }

    private void AssertComposerWasNotCalled() =>
        _composer.DidNotReceiveWithAnyArgs().Compose(default!, default, default);
}
