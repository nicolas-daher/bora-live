import type { ComponentProps } from 'react'
import styles from './Card.module.css'

/** Caixa de superfície para agrupar um conteúdo. */
export function Card({ className, ...props }: ComponentProps<'div'>) {
  return <div className={[styles.card, className].filter(Boolean).join(' ')} {...props} />
}
