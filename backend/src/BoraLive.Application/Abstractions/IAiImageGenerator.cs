using BoraLive.Domain.Common;

namespace BoraLive.Application.Abstractions;

/// <summary>
/// Porta para o serviço de IA de imagem (hoje, o Gemini).
/// A Application só conhece esta interface; quem a implementa é a Infrastructure.
/// </summary>
public interface IAiImageGenerator
{
    /// <summary>Pergunta ao serviço de IA se a chave do usuário é aceita.</summary>
    Task<ApiKeyStatus> ValidateKeyAsync(string apiKey, CancellationToken cancellationToken);

    /// <summary>
    /// Gera a imagem de fundo da arte a partir de um prompt.
    /// Falhas esperadas (chave recusada, serviço fora do ar, conteúdo recusado) voltam como erro, não como exceção.
    /// </summary>
    /// <param name="apiKey">A chave do usuário. Passa por aqui e não é guardada.</param>
    /// <param name="prompt">O pedido já montado, pronto para enviar à IA.</param>
    /// <param name="width">Largura da arte final. A IA escolhe a proporção suportada mais próxima.</param>
    /// <param name="height">Altura da arte final.</param>
    /// <returns>Os bytes da imagem gerada (PNG, JPEG ou WebP).</returns>
    Task<Result<byte[]>> GenerateBackgroundAsync(
        string apiKey,
        string prompt,
        int width,
        int height,
        CancellationToken cancellationToken);
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
