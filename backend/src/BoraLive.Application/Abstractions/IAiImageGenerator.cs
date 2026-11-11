namespace BoraLive.Application.Abstractions;

/// <summary>
/// Porta para o serviço de IA de imagem (hoje, o Gemini).
/// A Application só conhece esta interface; quem a implementa é a Infrastructure.
/// </summary>
public interface IAiImageGenerator
{
    /// <summary>Pergunta ao serviço de IA se a chave do usuário é aceita.</summary>
    Task<ApiKeyStatus> ValidateKeyAsync(string apiKey, CancellationToken cancellationToken);
}

/// <summary>O que o serviço de IA respondeu sobre uma chave.</summary>
public enum ApiKeyStatus
{
    /// <summary>A chave foi aceita.</summary>
    Valid,

    /// <summary>A chave foi recusada.</summary>
    Invalid,

    /// <summary>Não deu para saber: o serviço está fora do ar, lento ou limitando chamadas.</summary>
    Unavailable,
}
