import { useState, type FormEvent } from 'react'
import { validateApiKey } from '../../shared/api/apiKey.ts'
import { getApiKey, maskApiKey, removeApiKey, saveApiKey } from '../../shared/api/apiKeyStorage.ts'
import { ApiError } from '../../shared/api/client.ts'
import { Button } from '../../shared/ui/Button.tsx'
import { Card } from '../../shared/ui/Card.tsx'
import { Input } from '../../shared/ui/Input.tsx'
import { Page } from '../../shared/ui/Page.tsx'
import styles from './SettingsPage.module.css'

// O que a tela tem para dizer depois de uma ação.
type Feedback = { kind: 'success' | 'error'; message: string }

function describeError(error: unknown): string {
  if (!(error instanceof ApiError)) {
    return 'Opa, deu ruim por aqui. Tenta de novo?'
  }

  // A mensagem padrão da API manda "conferir nas configurações".
  // Como a pessoa já está nesta tela, o texto aqui é mais direto.
  if (error.code === 'api_key.invalid') {
    return 'Opa, essa chave não rolou. Confere se você copiou ela inteira?'
  }

  return error.message
}

export function SettingsPage() {
  // useState guarda um valor entre um desenho e outro da tela.
  // Quando o valor muda (pelo "set..."), o React redesenha o componente.
  const [draft, setDraft] = useState('') // o que está digitado no campo
  const [savedKey, setSavedKey] = useState(getApiKey) // a chave salva no navegador
  const [isTesting, setIsTesting] = useState(false)
  const [feedback, setFeedback] = useState<Feedback | null>(null)

  async function handleSubmit(event: FormEvent) {
    // Sem isto, o navegador recarregaria a página ao enviar o formulário.
    event.preventDefault()

    const apiKey = draft.trim()

    if (!apiKey) {
      setFeedback({ kind: 'error', message: 'Cola a sua chave do Gemini no campo antes de testar.' })
      return
    }

    setIsTesting(true)
    setFeedback(null)

    try {
      await validateApiKey(apiKey)

      // Só chega aqui se a chave for válida: chave ruim nunca é salva.
      if (saveApiKey(apiKey)) {
        setSavedKey(apiKey)
        setDraft('')
        setFeedback({ kind: 'success', message: 'Chave testada e salva. Partiu live!' })
      } else {
        setFeedback({
          kind: 'error',
          message: 'A chave funciona, mas o navegador não deixou salvar. Você tá em aba anônima?',
        })
      }
    } catch (error) {
      setFeedback({ kind: 'error', message: describeError(error) })
    } finally {
      setIsTesting(false)
    }
  }

  function handleRemove() {
    removeApiKey()
    setSavedKey(null)
    setFeedback({ kind: 'success', message: 'Chave removida deste navegador.' })
  }

  return (
    <Page
      title="Configurações"
      lead="Pra gerar imagens com IA, o Bora Live usa a sua própria chave do Gemini."
    >
      <div className={styles.layout}>
        <Card className={styles.panel}>
          <p className={styles.status}>
            {savedKey ? (
              <>
                <span className={styles.statusDot} />
                Chave salva neste navegador: <code className={styles.masked}>{maskApiKey(savedKey)}</code>
              </>
            ) : (
              'Nenhuma chave salva neste navegador.'
            )}
          </p>

          <form className={styles.form} onSubmit={handleSubmit}>
            <Input
              label={savedKey ? 'Trocar a chave' : 'Chave do Gemini'}
              hint="A chave é testada antes de ser salva."
              type="password"
              placeholder="Cole a chave aqui"
              autoComplete="off"
              spellCheck={false}
              value={draft}
              onChange={(event) => setDraft(event.target.value)}
              disabled={isTesting}
            />

            <div className={styles.actions}>
              <Button type="submit" disabled={isTesting}>
                {isTesting ? 'Testando…' : 'Testar chave'}
              </Button>
              {savedKey && (
                <Button variant="secondary" onClick={handleRemove} disabled={isTesting}>
                  Remover chave
                </Button>
              )}
            </div>
          </form>

          {/* role="status" faz o leitor de tela anunciar a mensagem quando ela aparece. */}
          <p role="status" className={feedback ? styles[feedback.kind] : undefined}>
            {feedback?.message}
          </p>
        </Card>

        <aside className={styles.notes}>
          <h2 className={styles.notesTitle}>Sua chave fica com você</h2>
          <ul className={styles.notesList}>
            <li>Ela fica salva só neste navegador, e você remove quando quiser.</li>
            <li>Ela vai pro servidor apenas na hora de cada pedido, pra falar com o Gemini.</li>
            <li>O servidor do Bora Live nunca salva nem registra a sua chave.</li>
          </ul>
          <p>
            Ainda não tem uma? Dá pra criar de graça no{' '}
            <a href="https://aistudio.google.com/apikey" target="_blank" rel="noreferrer">
              Google AI Studio
            </a>
            .
          </p>
        </aside>
      </div>
    </Page>
  )
}
