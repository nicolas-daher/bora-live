namespace BoraLive.Api.Http;

public static class ApiHeaders
{
    /// <summary>
    /// Header em que o frontend manda a chave do Gemini do usuário.
    /// A API só lê e repassa: nunca guarda, nem escreve em log.
    /// </summary>
    public const string GeminiApiKey = "X-Gemini-Api-Key";
}
