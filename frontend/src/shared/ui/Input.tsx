import { useId, type ComponentProps } from 'react'
import styles from './Input.module.css'

type InputProps = ComponentProps<'input'> & {
  label: string
  /** Texto de ajuda, mostrado abaixo do campo. */
  hint?: string
}

/** Campo de texto com rótulo e texto de ajuda opcionais. */
export function Input({ label, hint, className, ...props }: InputProps) {
  // useId gera um id único para ligar o <label> ao <input>.
  // Com isso, clicar no rótulo foca o campo, e leitores de tela anunciam o nome dele.
  const id = useId()
  const hintId = `${id}-hint`

  return (
    <div className={styles.field}>
      <label htmlFor={id} className={styles.label}>
        {label}
      </label>
      <input
        id={id}
        className={[styles.input, className].filter(Boolean).join(' ')}
        aria-describedby={hint ? hintId : undefined}
        {...props}
      />
      {hint && (
        <p id={hintId} className={styles.hint}>
          {hint}
        </p>
      )}
    </div>
  )
}
