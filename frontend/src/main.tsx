import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
// A ordem importa: os tokens vêm primeiro, porque o global.css já usa as variáveis.
import './shared/styles/tokens.css'
import './shared/styles/global.css'
import { App } from './app/App.tsx'

// Ponto de entrada: liga o React à <div id="root"> do index.html.
createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
