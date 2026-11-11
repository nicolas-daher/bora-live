// Cliente HTTP: o ÚNICO lugar do frontend que usa fetch.
// As telas chamam funções como validateApiKey(), nunca fetch direto.

import { getApiKey } from './apiKeyStorage.ts'

// Endereço da API. Vem da variável de ambiente VITE_API_URL
// (definida no docker-compose ou no deploy); sem ela, usa a API local.
const API_URL: string = import.meta.env.VITE_API_URL ?? 'http://localhost:8080'

/** Mesmo nome que o backend lê em ApiHeaders.GeminiApiKey. */
const API_KEY_HEADER = 'X-Gemini-Api-Key'

/** Erro devolvido pela API, já com a mensagem pronta para mostrar na tela. */
export class ApiError extends Error {
  /** Status HTTP. Zero quando nem deu para falar com o servidor. */
  readonly status: number
  /** Código estável do erro (ex.: "api_key.invalid"), para o código decidir o que fazer. */
  readonly code: string

  constructor(message: string, status: number, code: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.code = code
  }
}

type RequestOptions = {
  method?: 'GET' | 'POST'
  /** Chave a enviar. Sem ela, usa a que estiver salva no navegador. */
  apiKey?: string
}

/** Faz uma chamada à API. Lança ApiError quando a resposta não é de sucesso. */
export async function apiRequest(path: string, options: RequestOptions = {}): Promise<Response> {
  const headers = new Headers()
  const apiKey = options.apiKey ?? getApiKey()

  if (apiKey) {
    headers.set(API_KEY_HEADER, apiKey)
  }

  let response: Response

  try {
    response = await fetch(`${API_URL}${path}`, { method: options.method ?? 'GET', headers })
  } catch {
    // fetch só lança quando a requisição nem chegou: servidor fora do ar, sem internet.
    throw new ApiError('Não deu pra falar com o servidor do Bora Live. Tenta de novo daqui a pouco?', 0, 'network')
  }

  if (!response.ok) {
    throw await toApiError(response)
  }

  return response
}

// A API devolve erros no formato ProblemDetails: { title, status, code }.
async function toApiError(response: Response): Promise<ApiError> {
  const fallback = 'Opa, deu ruim por aqui. Tenta de novo?'

  try {
    const problem: { title?: string; code?: string } = await response.json()
    return new ApiError(problem.title ?? fallback, response.status, problem.code ?? 'unknown')
  } catch {
    // A resposta não era JSON (ex.: erro de um proxy no meio do caminho).
    return new ApiError(fallback, response.status, 'unknown')
  }
}
