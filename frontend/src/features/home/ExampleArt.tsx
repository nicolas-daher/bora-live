import styles from './ExampleArt.module.css'

/**
 * Exemplo ilustrativo de uma thumbnail, desenhado só com HTML e CSS.
 * O streamer e o título são fictícios, e não há elemento de nenhum jogo.
 * Quando o editor estiver pronto, isto dá lugar a uma arte gerada de verdade.
 */
export function ExampleArt() {
  return (
    <figure className={styles.figure}>
      {/* role="img" + aria-label: o leitor de tela anuncia uma descrição única,
          em vez de ler os pedaços soltos do desenho. */}
      <div
        className={styles.art}
        role="img"
        aria-label="Exemplo de thumbnail: selo ao vivo, o nome mare_alta e o título Subindo de elo, sem tilt, sobre um pôr do sol neon."
      >
        <div className={styles.sun} />
        <div className={styles.ground} />

        <div className={styles.content}>
          <span className={styles.badge}>
            <span className={styles.dot} />
            ao vivo
          </span>
          <p className={styles.title}>Subindo de elo, sem tilt</p>
          <p className={styles.streamer}>mare_alta</p>
        </div>
      </div>
      <figcaption className={styles.caption}>
        Exemplo ilustrativo de thumbnail, no tamanho 1280×720.
      </figcaption>
    </figure>
  )
}
