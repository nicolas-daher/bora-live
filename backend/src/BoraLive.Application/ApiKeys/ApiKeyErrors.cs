using BoraLive.Domain.Common;

namespace BoraLive.Application.ApiKeys;

public static class ApiKeyErrors
{
    public static readonly Error Missing = new(
        "api_key.missing",
        "Faltou a chave do Gemini. Cola ela lá nas configurações?");

    public static readonly Error Invalid = new(
        "api_key.invalid",
        "Opa, essa chave não rolou. Confere lá nas configurações?");

    public static readonly Error ProviderUnavailable = new(
        "api_key.provider_unavailable",
        "O Gemini não respondeu agora. Tenta de novo daqui a pouco?");
}
