namespace BoraLive.Domain.Templates;

/// <summary>
/// O papel de uma camada de texto: diz qual informação da arte ela mostra.
/// </summary>
public enum TextRole
{
    /// <summary>Nome do streamer.</summary>
    StreamerName,

    /// <summary>Título da live.</summary>
    LiveTitle,

    /// <summary>Chamada para a live (ex.: "tô ao vivo").</summary>
    CallToAction,
}
