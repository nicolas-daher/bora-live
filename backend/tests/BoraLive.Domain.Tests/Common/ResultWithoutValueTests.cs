using BoraLive.Domain.Common;

namespace BoraLive.Domain.Tests.Common;

public class ResultWithoutValueTests
{
    private static readonly Error SampleError = new("sample.error", "Deu ruim.");

    [Fact]
    public void Success_HasNoError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_CarriesError()
    {
        var result = Result.Failure(SampleError);

        Assert.True(result.IsFailure);
        Assert.Equal(SampleError, result.Error);
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailure()
    {
        Result result = SampleError;

        Assert.True(result.IsFailure);
        Assert.Equal(SampleError, result.Error);
    }
}
