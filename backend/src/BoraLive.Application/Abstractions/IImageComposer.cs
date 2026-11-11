using BoraLive.Domain.Art;
using BoraLive.Domain.Common;

namespace BoraLive.Application.Abstractions;

/// <summary>
/// Porta para compor a imagem final: fundo, textos e logo, no tamanho do formato.
/// A Application não sabe com que biblioteca a imagem é desenhada.
/// </summary>
public interface IImageComposer
{
    /// <summary>
    /// Desenha a arte e devolve o PNG.
    /// Falha com erro de negócio quando o logo enviado não é uma imagem que dê para abrir.
    /// </summary>
    /// <param name="request">O pedido, já validado pelo Domain.</param>
    /// <param name="logo">Bytes do logo do streamer, ou null quando não houver.</param>
    Result<byte[]> Compose(ArtRequest request, byte[]? logo);
}
