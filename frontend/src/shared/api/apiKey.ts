import { apiRequest } from './client.ts'

/**
 * Pede para a API testar uma chave do Gemini.
 * Termina sem erro se a chave for válida; lança ApiError se não for.
 */
export async function validateApiKey(apiKey: string): Promise<void> {
  await apiRequest('/api/key/validate', { method: 'POST', apiKey })
}
