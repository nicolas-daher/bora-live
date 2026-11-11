import type { ComponentProps } from 'react'
import { Link } from 'react-router'
import styles from './Button.module.css'

type Variant = 'primary' | 'secondary'

// Junta a classe base, a da variante e alguma classe extra que venha de fora.
function classNames(variant: Variant, extra?: string) {
  return [styles.button, styles[variant], extra].filter(Boolean).join(' ')
}

// ComponentProps<'button'> traz todas as props de um <button> comum
// (onClick, disabled, type...), sem precisar declarar uma por uma.
type ButtonProps = ComponentProps<'button'> & {
  variant?: Variant
}

/** Botão para ações na própria tela (gerar, testar, baixar). */
export function Button({ variant = 'primary', className, type = 'button', ...props }: ButtonProps) {
  return <button type={type} className={classNames(variant, className)} {...props} />
}

type ButtonLinkProps = ComponentProps<typeof Link> & {
  variant?: Variant
}

/**
 * Link com aparência de botão, para navegar entre telas.
 * É um componente separado porque navegar é papel de <a>, não de <button>:
 * assim "abrir em nova aba" e leitores de tela funcionam como esperado.
 */
export function ButtonLink({ variant = 'primary', className, ...props }: ButtonLinkProps) {
  return <Link className={classNames(variant, className)} {...props} />
}
