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

    /// <summary>Tempo máximo de espera por uma resposta, em segundos.</summary>
    public int TimeoutSeconds { get; set; } = 15;
}
