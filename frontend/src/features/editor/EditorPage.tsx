import { useEffect, useState, type ChangeEvent } from 'react'
import { Link, useSearchParams } from 'react-router'
import { generateArt } from '../../shared/api/art.ts'
import { listFormats, listTemplates, type ArtFormat, type Template } from '../../shared/api/catalog.ts'
import { paths } from '../../shared/paths.ts'
import { Button } from '../../shared/ui/Button.tsx'
import { Input } from '../../shared/ui/Input.tsx'
import { Page } from '../../shared/ui/Page.tsx'
import styles from './EditorPage.module.css'

// Mesmos limites do Domain (backend). A API continua sendo quem valida de verdade.
const MAX_STREAMER_NAME = 25
const MAX_LIVE_TITLE = 60
const MAX_LOGO_BYTES = 2 * 1024 * 1024

/** Passo 2: formulário à esquerda, prévia (o PNG real da API) à direita. */
export function EditorPage() {
  const [params] = useSearchParams()
  const formatKey = params.get('format')
  const gameId = params.get('game')

  const [format, setFormat] = useState<ArtFormat | null>(null)
  const [templates, setTemplates] = useState<Template[]>([])
  const [templateId, setTemplateId] = useState('')
  const [streamerName, setStreamerName] = useState('')
  const [liveTitle, setLiveTitle] = useState('')
  const [logo, setLogo] = useState<File | null>(null)

  const [artUrl, setArtUrl] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // Carrega formatos e modelos do jogo; só ficam os modelos que aceitam o formato escolhido.
  useEffect(() => {
    if (!formatKey || !gameId) return
    let cancelled = false
    Promise.all([listFormats(), listTemplates(gameId)])
      .then(([formats, all]) => {
        if (cancelled) return
        const usable = all.filter((t) => t.formats.includes(formatKey))
        setFormat(formats.find((f) => f.key === formatKey) ?? null)
        setTemplates(usable)
        setTemplateId(usable[0]?.id ?? '')
      })
      .catch((e: Error) => {
        if (!cancelled) setError(e.message)
      })
    return () => {
      cancelled = true
    }
  }, [formatKey, gameId])

  // Libera a URL temporária do Blob quando ela é trocada ou a tela fecha (senão vaza memória).
  useEffect(() => {
    return () => {
      if (artUrl) URL.revokeObjectURL(artUrl)
    }
  }, [artUrl])

  if (!formatKey || !gameId) {
    return (
      <Page title="Editor" lead="Faltou escolher o formato e o jogo.">
        <Link to={paths.create}>Voltar e escolher</Link>
      </Page>
    )
  }

  function onLogoChange(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0] ?? null
    if (file && file.size > MAX_LOGO_BYTES) {
      setError('Opa, esse logo é pesado demais. O limite é 2 MB.')
      event.target.value = ''
      setLogo(null)
      return
    }
    setError(null)
    setLogo(file)
  }

  async function generate() {
    setLoading(true)
    setError(null)
    try {
      const blob = await generateArt({ templateId, format: formatKey!, streamerName, liveTitle, logo })
      setArtUrl(URL.createObjectURL(blob))
    } catch (e) {
      setError((e as Error).message)
    } finally {
      setLoading(false)
    }
  }

  function download() {
    if (!artUrl) return
    const link = document.createElement('a')
    link.href = artUrl
    link.download = `bora-live-${templateId}-${formatKey}.png` // mesmo nome sugerido pela API
    link.click()
  }

  const canGenerate = !loading && templateId !== '' && streamerName.trim() !== '' && liveTitle.trim() !== ''

  return (
    <Page title="Editor" lead="Preenche os dados, clica em “Bora gerar” e a arte aparece ao lado.">
      <div className={styles.layout}>
        <form
          className={styles.form}
          onSubmit={(e) => {
            e.preventDefault()
            if (canGenerate) void generate()
          }}
        >
          <Input
            label="Nome do streamer"
            maxLength={MAX_STREAMER_NAME}
            value={streamerName}
            onChange={(e) => setStreamerName(e.target.value)}
          />
          <Input
            label="Título da live"
            maxLength={MAX_LIVE_TITLE}
            value={liveTitle}
            onChange={(e) => setLiveTitle(e.target.value)}
            hint="Emoji não aparece na arte (vira quadradinho), então evita."
          />
          <label className={styles.label}>
            Logo (opcional)
            <input type="file" accept="image/png,image/jpeg,image/webp" onChange={onLogoChange} />
            <span className={styles.hint}>PNG, JPG ou WebP, até 2 MB.</span>
          </label>
          <label className={styles.label}>
            Modelo
            <select className={styles.select} value={templateId} onChange={(e) => setTemplateId(e.target.value)}>
              {templates.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.name}
                </option>
              ))}
            </select>
          </label>

          {error && (
            <p className={styles.error} role="alert">
              {error}
            </p>
          )}

          <div className={styles.actions}>
            <Button type="submit" disabled={!canGenerate}>
              {loading ? 'Gerando…' : 'Bora gerar'}
            </Button>
            <Button variant="secondary" disabled={!artUrl} onClick={download}>
              Baixar PNG
            </Button>
          </div>
        </form>

        <div className={styles.preview}>
          {/* aspect-ratio reserva o tamanho certo do formato antes mesmo de gerar. */}
          <div
            className={styles.canvas}
            style={format ? { aspectRatio: `${format.width} / ${format.height}` } : undefined}
          >
            {artUrl ? <img src={artUrl} alt="Prévia da arte gerada" /> : <p>A prévia aparece aqui.</p>}
          </div>
          {artUrl && !error && <p className={styles.success}>Partiu live! Sua arte tá pronta.</p>}
        </div>
      </div>
    </Page>
  )
}
