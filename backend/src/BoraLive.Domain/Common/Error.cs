namespace BoraLive.Domain.Common;

/// <summary>
/// Um erro de negócio esperado.
/// </summary>
/// <param name="Code">Identificador estável, para o código tomar decisões (ex.: "art_request.title_too_long").</param>
/// <param name="Message">Texto para a pessoa, em português e no tom do Bora Live.</param>
public sealed record Error(string Code, string Message);
