using BoraLive.Application.ApiKeys;
using BoraLive.Domain.Common;

namespace BoraLive.Api.Http;

/// <summary>
/// Converte um erro de negócio em resposta HTTP no formato ProblemDetails.
/// </summary>
public static class ErrorResults
{
    // Só a Api sabe de HTTP: é aqui que cada erro ganha um status.
    // Erro que não estiver na lista vira 400 (requisição inválida).
    private static readonly Dictionary<string, int> StatusByErrorCode = new()
    {
        [ApiKeyErrors.Missing.Code] = StatusCodes.Status400BadRequest,
        [ApiKeyErrors.Invalid.Code] = StatusCodes.Status401Unauthorized,
        [ApiKeyErrors.ProviderUnavailable.Code] = StatusCodes.Status503ServiceUnavailable,
    };

    public static IResult ToProblem(this Error error)
    {
        var status = StatusByErrorCode.GetValueOrDefault(error.Code, StatusCodes.Status400BadRequest);

        return Results.Problem(
            title: error.Message,
            statusCode: status,
            // "code" é um campo extra: o frontend usa para decidir o que fazer,
            // sem depender do texto da mensagem.
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }
}
