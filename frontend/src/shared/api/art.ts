import { apiRequest } from './client.ts'

export type ArtParams = {
  templateId: string
  format: string
  streamerName: string
  liveTitle: string
  logo?: File | null
  /** O que a pessoa quer no fundo. Sem isso (ou sem chave), a arte usa o degradê do modelo. */
  prompt?: string
}

export type GeneratedArt = {
  png: Blob
  /** Código do aviso quando a IA não rolou e a arte saiu com o degradê; null quando deu tudo certo. */
  warning: string | null
}

/** Mesmo nome que o backend expõe em ApiHeaders.BoraWarning. */
const WARNING_HEADER = 'X-Bora-Warning'

/** Pede a arte pronta à API e devolve o PNG como Blob. */
export async function generateArt(params: ArtParams): Promise<GeneratedArt> {
  // Os nomes dos campos precisam bater com os [FromForm] do ArtEndpoints.
  const form = new FormData()
  form.set('templateId', params.templateId)
  form.set('format', params.format)
  form.set('streamerName', params.streamerName)
  form.set('liveTitle', params.liveTitle)
  if (params.logo) {
    form.set('logo', params.logo)
  }
  if (params.prompt?.trim()) {
    form.set('prompt', params.prompt.trim())
  }

  // A chave do Gemini vai no header sozinha: o apiRequest lê a que está salva no navegador.
  const response = await apiRequest('/api/art', { method: 'POST', body: form })

  return { png: await response.blob(), warning: response.headers.get(WARNING_HEADER) }
}

/** Mensagem (no tom do Bora Live) para cada código de aviso que a API pode mandar. */
export function warningMessage(code: string): string {
  switch (code) {
    case 'api_key.missing':
      return 'Pra usar a IA precisa da chave do Gemini. Cola ela lá nas configurações! Por enquanto, a arte saiu com o degradê.'
    case 'api_key.invalid':
      return 'Opa, essa chave não rolou. Confere lá nas configurações? A arte saiu com o degradê.'
    case 'art.ai_rejected':
      return 'A IA não topou gerar esse fundo. Tenta descrever de outro jeito? A arte saiu com o degradê.'
    default:
      return 'A IA não respondeu agora, então a arte saiu com o degradê. Tenta de novo daqui a pouco?'
  }
}
