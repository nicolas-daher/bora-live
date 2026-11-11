// Guarda a chave do Gemini SÓ no navegador do usuário (localStorage).
// Ela nunca é salva no servidor: vai em um header, a cada requisição.

const STORAGE_KEY = 'bora-live:gemini-api-key'

// O localStorage pode falhar (aba anônima, armazenamento bloqueado).
// Por isso cada acesso fica em try/catch: a tela continua funcionando,
// só não lembra da chave.

export function getApiKey(): string | null {
  try {
    return localStorage.getItem(STORAGE_KEY)
  } catch {
    return null
  }
}

/** Devolve true se conseguiu salvar. */
export function saveApiKey(apiKey: string): boolean {
  try {
    localStorage.setItem(STORAGE_KEY, apiKey)
    return true
  } catch {
    return false
  }
}

export function removeApiKey(): void {
  try {
    localStorage.removeItem(STORAGE_KEY)
  } catch {
    // Sem acesso ao armazenamento não há o que remover.
  }
}

/** Versão para mostrar na tela: só os 4 últimos caracteres. */
export function maskApiKey(apiKey: string): string {
  return `••••${apiKey.slice(-4)}`
}
