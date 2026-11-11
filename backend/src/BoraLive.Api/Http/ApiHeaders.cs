namespace BoraLive.Api.Http;

public static class ApiHeaders
{
    /// <summary>
    /// Header em que o frontend manda a chave do Gemini do usuário.
    /// A API só lê e repassa: nunca guarda, nem escreve em log.
    /// </summary>
    public const string GeminiApiKey = "X-Gemini-Api-Key";

    /// <summary>
    /// Header de resposta com o código do aviso, quando a arte saiu com o degradê porque a IA não rolou
    /// (ex.: "art.ai_unavailable"). Só o código: a mensagem em português fica a cargo do frontend,
    /// porque headers HTTP não são lugar para acentos.
    /// </summary>
    public const string BoraWarning = "X-Bora-Warning";
}
