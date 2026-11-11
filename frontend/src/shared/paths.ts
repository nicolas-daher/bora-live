// Endereços das telas em um lugar só.
// Assim nenhum componente escreve "/criar" à mão, e renomear uma rota é mudar uma linha.
export const paths = {
  home: '/',
  create: '/criar',
  editor: '/editor',
  settings: '/configuracoes',
  about: '/sobre',
} as const
