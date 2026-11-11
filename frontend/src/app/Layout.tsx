import { Link, NavLink, Outlet } from 'react-router'
import logo from '../shared/assets/logo-bora-live.svg'
import { paths } from '../shared/paths.ts'
import styles from './Layout.module.css'

const navItems = [
  { to: paths.create, label: 'Criar' },
  { to: paths.settings, label: 'Configurações' },
  { to: paths.about, label: 'Sobre' },
]

/** Moldura de todas as telas: cabeçalho, conteúdo da rota atual e rodapé. */
export function Layout() {
  return (
    <div className={styles.shell}>
      <header className={styles.header}>
        <div className={styles.headerInner}>
          <Link to={paths.home} className={styles.brand}>
            <img src={logo} alt="Bora Live, ir para o início" width={534} height={120} />
          </Link>

          <nav aria-label="Principal">
            <ul className={styles.navList}>
              {navItems.map((item) => (
                <li key={item.to}>
                  {/* NavLink sabe se o endereço atual é o dele (isActive). */}
                  <NavLink
                    to={item.to}
                    className={({ isActive }) =>
                      isActive ? `${styles.navLink} ${styles.navLinkActive}` : styles.navLink
                    }
                  >
                    {item.label}
                  </NavLink>
                </li>
              ))}
            </ul>
          </nav>
        </div>
      </header>

      <main className={styles.main}>
        {/* Aqui entra a tela da rota atual. */}
        <Outlet />
      </main>

      <footer className={styles.footer}>
        <p className={styles.footerInner}>
          Projeto pessoal, open source e sem fins comerciais. Não é oficial e não tem vínculo com as
          empresas dos jogos.
        </p>
      </footer>
    </div>
  )
}
