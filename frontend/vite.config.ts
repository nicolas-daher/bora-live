import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // host: true faz o servidor aceitar conexões de fora do container Docker.
    host: true,
    port: 5173,
    // Se a porta estiver ocupada, falha em vez de trocar de porta em silêncio.
    strictPort: true,
  },
})
