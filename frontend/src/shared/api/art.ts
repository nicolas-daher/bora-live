import { apiRequest } from './client.ts'

export type ArtParams = {
  templateId: string
  format: string
  streamerName: string
  liveTitle: string
  logo?: File | null
}

/** Pede a arte pronta à API e devolve o PNG como Blob. */
export async function generateArt(params: ArtParams): Promise<Blob> {
  // Os nomes dos campos precisam bater com os [FromForm] do ArtEndpoints.
  const form = new FormData()
  form.set('templateId', params.templateId)
  form.set('format', params.format)
  form.set('streamerName', params.streamerName)
  form.set('liveTitle', params.liveTitle)
  if (params.logo) {
    form.set('logo', params.logo)
  }

  const response = await apiRequest('/api/art', { method: 'POST', body: form })
  return response.blob()
}
