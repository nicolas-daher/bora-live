namespace BoraLive.Infrastructure.Gemini;

/// <summary>
/// Configurações do Gemini, lidas da seção "Gemini" do appsettings.json.
/// Aqui NÃO entra chave de API: a chave é sempre a do usuário, a cada requisição.
/// </summary>
public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    /// <summary>Endereço base da API do Gemini.</summary>
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/";

    /// <summary>Modelo que gera as imagens. Fica em configuração porque os nomes mudam com frequência.</summary>
    public string ImageModel { get; set; } = "gemini-2.5-flash-image";

    /// <summary>Tempo máximo para validar a chave, em segundos.</summary>
    public int TimeoutSeconds { get; set; } = 15;

    /// <summary>Tempo máximo para gerar uma imagem, em segundos. Gerar demora bem mais que validar.</summary>
    public int ImageTimeoutSeconds { get; set; } = 90;
}
