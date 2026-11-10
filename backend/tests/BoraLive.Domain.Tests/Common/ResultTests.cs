using BoraLive.Domain.Common;

namespace BoraLive.Domain.Tests.Common;

public class ResultTests
{
    private static readonly Error SampleError = new("sample.error", "Deu ruim.");

    [Fact]
    public void Success_CarriesValueAndNoError()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_CarriesError()
    {
        var result = Result<int>.Failure(SampleError);

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal(SampleError, result.Error);
    }

    [Fact]
    public void Value_OnFailure_Throws()
    {
        var result = Result<int>.Failure(SampleError);

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void ImplicitConversions_CreateSuccessAndFailure()
    {
        Result<string> success = "bora";
        Result<string> failure = SampleError;

        Assert.True(success.IsSuccess);
        Assert.Equal("bora", success.Value);
        Assert.True(failure.IsFailure);
        Assert.Equal(SampleError, failure.Error);
    }
}
