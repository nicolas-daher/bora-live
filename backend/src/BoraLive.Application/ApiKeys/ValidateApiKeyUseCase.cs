using BoraLive.Application.Abstractions;
using BoraLive.Domain.Common;

namespace BoraLive.Application.ApiKeys;

/// <summary>
/// Caso de uso: conferir se a chave do Gemini informada pelo usuário funciona.
/// A chave só passa por aqui: não é guardada, nem escrita em log.
/// </summary>
// Construtor primário: o parâmetro "aiImageGenerator" fica disponível na classe inteira.
// Quem entrega a implementação é a injeção de dependência, configurada na Api.
public sealed class ValidateApiKeyUseCase(IAiImageGenerator aiImageGenerator)
{
    /// <summary>Tamanho máximo aceito. As chaves do Gemini têm bem menos que isso.</summary>
    public const int MaxKeyLength = 200;

    public async Task<Result> ExecuteAsync(string? apiKey, CancellationToken cancellationToken = default)
    {
        var key = apiKey?.Trim();

        if (string.IsNullOrEmpty(key))
        {
            return ApiKeyErrors.Missing;
        }

        // Uma chave com espaço, acento ou quebra de linha nunca é válida.
        // Recusar aqui evita uma chamada à toa e impede que caracteres estranhos
        // cheguem ao header da requisição para o Gemini.
        if (key.Length > MaxKeyLength || !key.All(IsAllowedKeyCharacter))
        {
            return ApiKeyErrors.Invalid;
        }

        var status = await aiImageGenerator.ValidateKeyAsync(key, cancellationToken);

        return status switch
        {
            ApiKeyStatus.Valid => Result.Success(),
            ApiKeyStatus.Invalid => ApiKeyErrors.Invalid,
            _ => ApiKeyErrors.ProviderUnavailable,
        };
    }

    private static bool IsAllowedKeyCharacter(char c) =>
        char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.';
}
