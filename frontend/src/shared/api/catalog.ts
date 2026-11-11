import { apiRequest } from './client.ts'

// Os tipos espelham os DTOs do backend (FormatDto, GameDto, TemplateDto).
export type ArtFormat = { key: string; name: string; width: number; height: number }
export type Game = { id: string; name: string }
export type Template = {
  id: string
  name: string
  gameId: string
  /** Chaves dos formatos que o modelo aceita (ex.: "thumbnail"). */
  formats: string[]
  background: { colors: string[]; angle: number }
}

export async function listFormats(): Promise<ArtFormat[]> {
  return (await apiRequest('/api/formats')).json()
}

export async function listGames(): Promise<Game[]> {
  return (await apiRequest('/api/games')).json()
}

export async function listTemplates(gameId: string): Promise<Template[]> {
  return (await apiRequest(`/api/templates?game=${encodeURIComponent(gameId)}`)).json()
}
