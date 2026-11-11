import { BrowserRouter, Route, Routes } from 'react-router'
import { AboutPage } from '../features/about/AboutPage.tsx'
import { EditorPage } from '../features/editor/EditorPage.tsx'
import { GameSelectionPage } from '../features/game-selection/GameSelectionPage.tsx'
import { HomePage } from '../features/home/HomePage.tsx'
import { SettingsPage } from '../features/settings/SettingsPage.tsx'
import { paths } from '../shared/paths.ts'
import { Layout } from './Layout.tsx'
import { NotFoundPage } from './NotFoundPage.tsx'

/**
 * Raiz da aplicação: o mapa "endereço -> tela".
 * Todas as rotas ficam dentro do <Layout>, que desenha o cabeçalho e o rodapé
 * e mostra a tela da vez no lugar do <Outlet />.
 */
export function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route path={paths.home} element={<HomePage />} />
          <Route path={paths.create} element={<GameSelectionPage />} />
          <Route path={paths.editor} element={<EditorPage />} />
          <Route path={paths.settings} element={<SettingsPage />} />
          <Route path={paths.about} element={<AboutPage />} />
          {/* O asterisco pega qualquer endereço que não bateu com os de cima. */}
          <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}
