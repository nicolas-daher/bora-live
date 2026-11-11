import type { ReactNode } from 'react'
import styles from './Page.module.css'

type PageProps = {
  title: string
  lead?: string
  /** O conteúdo da tela: tudo o que for escrito entre <Page> e </Page>. */
  children?: ReactNode
}

/** Moldura padrão das telas internas: título, texto de apoio e conteúdo. */
export function Page({ title, lead, children }: PageProps) {
  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <h1 className={styles.title}>{title}</h1>
        {lead && <p className={styles.lead}>{lead}</p>}
      </header>
      {children}
    </div>
  )
}
