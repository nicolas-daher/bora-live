import { Card } from '../../shared/ui/Card.tsx'
import { Page } from '../../shared/ui/Page.tsx'
import styles from './AboutPage.module.css'

const repositoryUrl = 'https://github.com/nicolas-daher/bora-live'

export function AboutPage() {
  return (
    <Page
      title="Sobre o Bora Live"
      lead="Uma ferramenta open source pra streamer pequeno ter arte de live sem precisar abrir um editor de imagem."
    >
      <div className={styles.grid}>
        <Card className={styles.block}>
          <h2 className={styles.blockTitle}>O que é</h2>
          <p>
            O Bora Live gera thumbnails, banners e posts de “tô ao vivo” a partir de modelos por
            jogo. É um projeto pessoal, feito pra aprender e pra ajudar quem tá começando a
            streamar.
          </p>
        </Card>

        <Card className={styles.block}>
          <h2 className={styles.blockTitle}>Não é oficial nem comercial</h2>
          <p>
            O projeto não tem vínculo com a Valve, com a Riot Games nem com nenhuma outra empresa
            dos jogos citados. Os nomes dos jogos pertencem aos seus donos e aparecem só em texto, pra
            identificar os modelos. O Bora Live não usa logos, personagens nem artes oficiais.
          </p>
        </Card>

        <Card className={styles.block}>
          <h2 className={styles.blockTitle}>Sua chave fica com você</h2>
          <p>
            Pra gerar imagens com IA, você usa a sua própria chave do Gemini. Ela fica salva só no
            seu navegador e nunca é guardada no servidor.
          </p>
        </Card>

        <Card className={styles.block}>
          <h2 className={styles.blockTitle}>Código aberto</h2>
          <p>
            O código tá no <a href={repositoryUrl}>GitHub</a>, sob a{' '}
            <a href={`${repositoryUrl}/blob/main/LICENSE`}>licença MIT</a>. Pode ler, usar e
            adaptar.
          </p>
        </Card>
      </div>
    </Page>
  )
}
