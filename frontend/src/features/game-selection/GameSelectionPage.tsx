import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router'
import { listFormats, listGames, type ArtFormat, type Game } from '../../shared/api/catalog.ts'
import { paths } from '../../shared/paths.ts'
import { Button } from '../../shared/ui/Button.tsx'
import { Page } from '../../shared/ui/Page.tsx'
import styles from './GameSelectionPage.module.css'

/**
 * Passo 1: escolher formato e jogo.
 * A escolha vai para o editor pela query string (/editor?format=...&game=...):
 * sobrevive ao F5 e o endereço pode ser compartilhado.
 */
export function GameSelectionPage() {
  const navigate = useNavigate()
  const [formats, setFormats] = useState<ArtFormat[]>([])
  const [games, setGames] = useState<Game[]>([])
  const [format, setFormat] = useState<string | null>(null)
  const [game, setGame] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false // evita atualizar o estado se a tela já foi fechada
    Promise.all([listFormats(), listGames()])
      .then(([f, g]) => {
        if (cancelled) return
        setFormats(f)
        setGames(g)
      })
      .catch((e: Error) => {
        if (!cancelled) setError(e.message)
      })
    return () => {
      cancelled = true
    }
  }, [])

  function goToEditor() {
    if (!format || !game) return
    navigate(`${paths.editor}?${new URLSearchParams({ format, game })}`)
  }

  return (
    <Page title="Escolha o formato e o jogo" lead="Bora começar: primeiro o formato da arte, depois o jogo.">
      {error && <p className={styles.error}>{error}</p>}

      <section className={styles.section}>
        <h2 className={styles.heading}>1. Formato</h2>
        <div className={styles.grid}>
          {formats.map((f) => (
            <button
              key={f.key}
              type="button"
              className={styles.option}
              aria-pressed={format === f.key}
              onClick={() => setFormat(f.key)}
            >
              <span className={styles.optionName}>{f.name}</span>
              <span className={styles.optionMeta}>
                {f.width}×{f.height}
              </span>
            </button>
          ))}
        </div>
      </section>

      <section className={styles.section}>
        <h2 className={styles.heading}>2. Jogo</h2>
        <div className={styles.grid}>
          {games.map((g) => (
            <button
              key={g.id}
              type="button"
              className={styles.option}
              aria-pressed={game === g.id}
              onClick={() => setGame(g.id)}
            >
              <span className={styles.optionName}>{g.name}</span>
            </button>
          ))}
        </div>
      </section>

      <div className={styles.actions}>
        <Button disabled={!format || !game} onClick={goToEditor}>
          Partiu pro editor
        </Button>
      </div>
    </Page>
  )
}
