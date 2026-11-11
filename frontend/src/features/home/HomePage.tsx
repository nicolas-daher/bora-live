import type { CSSProperties } from 'react'
import { paths } from '../../shared/paths.ts'
import { ButtonLink } from '../../shared/ui/Button.tsx'
import { ExampleArt } from './ExampleArt.tsx'
import styles from './HomePage.module.css'

const steps = [
  {
    title: 'Escolha o formato e o jogo',
    text: 'Thumbnail, banner, post ou story, com um modelo pensado pro seu jogo.',
  },
  {
    title: 'Coloque seu nome e o título',
    text: 'Suba seu logo se quiser. A prévia muda na hora, do lado.',
  },
  {
    title: 'Baixe o PNG',
    text: 'A arte sai no tamanho certo da plataforma, pronta pra postar.',
  },
]

// Por enquanto a lista fica aqui. Na etapa 6 ela passa a vir da API,
// que é a dona dessa informação (ArtFormat, no Domain).
const formats = [
  { name: 'Thumbnail', platform: 'YouTube', width: 1280, height: 720 },
  { name: 'Banner de perfil', platform: 'Twitch', width: 1200, height: 480 },
  { name: 'Post quadrado', platform: 'Instagram e X', width: 1080, height: 1080 },
  { name: 'Story', platform: 'Instagram', width: 1080, height: 1920 },
]

export function HomePage() {
  return (
    <div className={styles.home}>
      <section className={styles.hero}>
        <div className={styles.heroText}>
          <h1 className={styles.heroTitle}>Sua live com cara de live, em poucos cliques</h1>
          <p className={styles.heroLead}>
            O Bora Live monta thumbnail, banner e post de “tô ao vivo” a partir de modelos por jogo.
            Você entra com o nome e o título, e sai com a arte pronta.
          </p>
          <div className={styles.heroActions}>
            <ButtonLink to={paths.create}>Bora criar</ButtonLink>
            <ButtonLink to={paths.about} variant="secondary">
              Conhecer o projeto
            </ButtonLink>
          </div>
        </div>

        <ExampleArt />
      </section>

      <section className={styles.section} aria-labelledby="como-funciona">
        <h2 id="como-funciona" className={styles.sectionTitle}>
          Como funciona
        </h2>
        {/* <ol> porque é uma sequência de verdade: a ordem dos passos importa. */}
        <ol className={styles.steps}>
          {steps.map((step) => (
            <li key={step.title} className={styles.step}>
              <h3 className={styles.stepTitle}>{step.title}</h3>
              <p className={styles.stepText}>{step.text}</p>
            </li>
          ))}
        </ol>
      </section>

      <section className={styles.section} aria-labelledby="formatos">
        <h2 id="formatos" className={styles.sectionTitle}>
          Quatro formatos, cada um no tamanho da plataforma
        </h2>
        <ul className={styles.formats}>
          {formats.map((format) => (
            <li key={format.name} className={styles.format}>
              {/* A moldura é desenhada na proporção real do formato.
                  A proporção vai para o CSS por uma variável (--ratio). */}
              <div className={styles.frameArea}>
                <div
                  className={styles.frame}
                  style={{ '--ratio': format.width / format.height } as CSSProperties}
                />
              </div>
              <p className={styles.formatName}>{format.name}</p>
              <p className={styles.formatMeta}>
                {format.platform}, {format.width}×{format.height}
              </p>
            </li>
          ))}
        </ul>
      </section>
    </div>
  )
}
