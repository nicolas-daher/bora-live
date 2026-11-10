# CLAUDE.md — Bora Live

Briefing do projeto para o Claude Code. Leia este arquivo antes de qualquer tarefa.

## O que é

**Bora Live** é uma plataforma open source que gera thumbnails, banners e posts de "tô ao vivo" para streamers pequenos, a partir de modelos por jogo (ex.: CS2, Valorant).

- Não é comercial. O objetivo é aprender, testar o desenvolvimento com o Claude Code e servir de vitrine no GitHub e no LinkedIn.
- Público: streamers e criadores de games, principalmente pequenos e brasileiros, que precisam de visual constante e não dominam design.
- Modelo de custo: "traga sua própria chave". O usuário usa a chave do Gemini dele; o projeto não paga API.

## Como trabalhar comigo (o dono do projeto)

- Sou dev pleno vindo de Oracle (PL/SQL, Forms, Reports), migrando para C#/.NET e React. Tenho pós em Engenharia de Software .NET.
- **Explique as decisões** enquanto implementa: por que escolheu tal padrão, biblioteca ou estrutura, e qual alternativa descartou. Explicações curtas e práticas, ligadas ao código que está sendo escrito.
- Antes de uma mudança grande (nova camada, nova dependência, mudança de arquitetura), apresente o plano e espere confirmação.
- Mantenha o MVP enxuto. Não adicione funcionalidades fora do escopo abaixo sem perguntar.

## Idioma

Tudo em **português do Brasil**: interface, textos, README, documentação, mensagens de commit e descrições de PR.

- Nomes de código (classes, métodos, variáveis, pastas) em **inglês**, seguindo as convenções de C# e React.
- Commits no formato Conventional Commits, com descrição em português. Ex.: `feat: adiciona modelo de thumbnail de CS2`.

## Identidade visual

**Clima:** tropical neon, de fim de tarde. Identidade própria — **nenhum** elemento de GTA ou de qualquer outro jogo na marca.

### Logo
- Símbolo: sol se pondo no horizonte, com um ponto turquesa de "ao vivo".
- Wordmark: `bora live`, em minúsculas, com "live" em rosa neon.
- O degradê de pôr do sol aparece só no símbolo e em destaques pontuais.

### Arquivos da marca
- O logo oficial é `brand/logo-bora-live.svg`, com o símbolo e o wordmark. O texto já está convertido em curvas, então não depende da fonte instalada.
- A origem dos arquivos é a pasta `G:\My Drive\Projeto Pessoal\Imagens`, no Google Drive do dono do projeto. Ela fica fora do repositório. Copie os arquivos para a pasta `brand/` na raiz do repositório e versione-os ali.
- Nunca referencie o caminho `G:\` no código, no Docker ou no CI. O código usa só os arquivos de `brand/`.
- **No frontend:** copie ou importe o SVG em `frontend/src/shared/assets/`. Use o logo completo no cabeçalho e na tela Início.
- **Favicon e ícones:** gere uma versão só com o símbolo (sol e ponto turquesa), recortando o `viewBox` para a área do símbolo (`0 0 120 120`). Salve como `brand/simbolo-bora-live.svg` e use como favicon.
- Não altere as cores nem as proporções do logo. Se precisar de uma variação, como monocromática ou para fundo claro, pergunte antes.

### Paleta

| Token | Nome | Hex | Uso |
|---|---|---|---|
| `--color-night` | Noite | `#1A1033` | Fundo principal |
| `--color-pink` | Rosa neon | `#FF4F8B` | Destaque, links, "live" do logo |
| `--color-orange` | Laranja | `#FF9A3D` | Ações, botões primários |
| `--color-sun` | Amarelo sol | `#FFD166` | Brilho, realces, foco |
| `--color-teal` | Turquesa | `#2EC4B6` | Indicador "ao vivo", status positivo |

- Degradê de pôr do sol: `linear-gradient(135deg, #FFD166, #FF9A3D, #FF4F8B)` — usar com moderação.
- Tema escuro por padrão (fundo Noite). Derivar tons de superfície (cards, inputs, bordas) a partir do Noite, um pouco mais claros.
- Garantir contraste AA para textos sobre o fundo escuro.
- Definir as cores como variáveis CSS em um único arquivo de tokens; nunca escrever hex solto nos componentes.

### Tipografia
- **Unbounded** — logo e títulos.
- **Inter** — interface, textos e formulários.
- Ambas pelo Google Fonts.

### Tom de voz
Animado, direto e brasileiro: "bora", "chat", "partiu". Nunca técnico demais. Exemplos:
- Botão de gerar: "Bora gerar"
- Sucesso: "Partiu live! Sua arte tá pronta."
- Erro de chave: "Opa, essa chave não rolou. Confere lá nas configurações?"

## Funcionalidades do MVP

1. **Modelos por jogo** — thumbnail e banner com layout pronto para cada jogo suportado (começar com CS2 e Valorant).
2. **Post de "tô ao vivo"** — arte com nome do streamer, jogo, logo do streamer e chamada para a live.
3. **Geração de imagem com IA** — fundo ou elementos gerados pelo Gemini, usando a chave do próprio usuário.
4. **Configurações** — tela para colar, testar e remover a chave da API do Gemini.
5. **Download** — baixar a arte final em PNG.

### Formatos de saída (conferir as medidas atuais de cada plataforma)
- Thumbnail (YouTube): 1280×720
- Banner de perfil (Twitch): 1200×480
- Post quadrado (Instagram/X): 1080×1080
- Story (Instagram): 1080×1920

### Fora do escopo do MVP
Contas de usuário, login, banco de dados de usuários, histórico salvo no servidor, pagamentos, agendamento de posts, integração direta com Twitch/YouTube.

## Telas

1. **Início** — apresentação curta do Bora Live, exemplo de arte gerada e botão "Bora criar".
2. **Escolher formato e jogo** — cards de formato (thumbnail, banner, post ao vivo, story) e grade de jogos. O logo do jogo só identifica a opção.
3. **Editor** — à esquerda, formulário (nome do streamer, título da live, upload do logo, escolha do modelo, prompt opcional para a IA); à direita, prévia em tempo real. Botões "Bora gerar" e "Baixar PNG".
4. **Configurações** — campo para colar a chave do Gemini, botão "Testar chave", botão "Remover chave" e um texto claro explicando que a chave não é salva no servidor.
5. **Sobre** — o que é o projeto, aviso de que não é oficial nem comercial, link para o GitHub e licença.

Layout responsivo, mas o editor pode priorizar desktop.

## Arquitetura

O backend segue **Clean Architecture**. O frontend segue a mesma ideia de separação, em versão mais leve.

Monorepo:

```
/
├── frontend/                          # React + TypeScript + Vite
├── backend/
│   ├── BoraLive.sln
│   ├── src/
│   │   ├── BoraLive.Domain/           # Núcleo: entidades e regras de negócio
│   │   ├── BoraLive.Application/      # Casos de uso e interfaces (portas)
│   │   ├── BoraLive.Infrastructure/   # Implementações: Gemini, SkiaSharp, JSON
│   │   └── BoraLive.Api/              # ASP.NET Core: endpoints, DI, middleware
│   ├── tests/
│   │   ├── BoraLive.Domain.Tests/
│   │   ├── BoraLive.Application.Tests/
│   │   └── BoraLive.Api.Tests/        # Testes de integração
│   └── Dockerfile
├── brand/                             # Logo e arquivos da marca (SVG)
├── docker-compose.yml
├── CLAUDE.md
├── README.md
└── LICENSE                            # MIT
```

### Regra de dependência (inegociável)

As dependências apontam **sempre para dentro**:

```
Api ──► Infrastructure ──► Application ──► Domain
 └──────────────────────► Application
```

- **Domain** não referencia nenhum outro projeto nem pacote de framework (sem ASP.NET, sem SkiaSharp, sem HttpClient).
- **Application** referencia só o Domain. Define as interfaces que precisa e não sabe quem as implementa.
- **Infrastructure** implementa as interfaces da Application. É o único lugar com SkiaSharp, chamadas HTTP ao Gemini e leitura de arquivos.
- **Api** é a camada de entrada e o *composition root*: registra as dependências (injeção de dependência) e expõe os endpoints.
- Se uma mudança exigir quebrar essa regra, pare e explique antes de fazer.

### Responsabilidade de cada camada

**Domain**
- Entidades e objetos de valor: `GameTemplate` (modelo por jogo), `ArtFormat` (thumbnail, banner, post, story, com largura e altura), `TextLayer`, `ArtRequest`.
- Regras de negócio puras. Ex.: um formato tem dimensões válidas; o nome do streamer tem tamanho máximo; um modelo só aceita os formatos que declara.
- Validação nos próprios construtores ou métodos de fábrica, para que um objeto inválido não exista.

**Application**
- Casos de uso, um por classe: `ListGamesUseCase`, `ListTemplatesUseCase`, `GenerateArtUseCase`, `ValidateApiKeyUseCase`.
- Interfaces (portas) que a Infrastructure implementa:
  - `ITemplateRepository` — buscar jogos e modelos.
  - `IImageComposer` — compor a imagem final (texto, logo, layout, fundo).
  - `IAiImageGenerator` — gerar a imagem com IA a partir de um prompt e da chave do usuário.
- DTOs de entrada e saída dos casos de uso. Os endpoints nunca expõem entidades do Domain diretamente.
- Erros de negócio retornados como resultado (ex.: um tipo `Result<T>`), não como exceção, quando o erro for esperado (chave inválida, modelo inexistente).

**Infrastructure**
- `JsonTemplateRepository` — lê os modelos por jogo de arquivos JSON versionados no repositório (sem banco de dados no MVP).
- `SkiaImageComposer` — composição com **SkiaSharp** (licença MIT).
- `GeminiImageGenerator` — chamadas ao Gemini via `HttpClient` tipado. Nome do modelo do Gemini vem de configuração, nunca fixo no código.
- Um método de extensão `AddInfrastructure()` para registrar tudo na injeção de dependência.

**Api**
- ASP.NET Core com **Minimal APIs**, endpoints agrupados por recurso (`/api/games`, `/api/templates`, `/api/art`).
- Lê a chave do Gemini do header e a repassa ao caso de uso; nunca a guarda.
- Middleware de tratamento de erros, que converte falhas em respostas `ProblemDetails` com mensagens no tom do Bora Live.
- CORS liberado só para o domínio do frontend.
- Swagger/OpenAPI ativo em desenvolvimento.

### O que **não** usar (manter o MVP enxuto)
- Sem MediatR: casos de uso são classes simples injetadas nos endpoints.
- Sem CQRS, Event Sourcing ou Unit of Work: não há banco de dados no MVP.
- Sem AutoMapper: mapeamentos manuais e explícitos, fáceis de ler.
- Clean Architecture aqui serve para separar responsabilidades e facilitar testes, não para criar camadas vazias. Se uma abstração não tiver uso real, não crie.

### Backend — hospedagem
- **C# / ASP.NET Core**, versão LTS mais recente do .NET.
- Roda em container Docker (deploy em Render, Fly.io ou Azure App Service — a decidir).

### Frontend
- React + TypeScript + Vite, hospedado no **Cloudflare Pages** (deploy automático a cada push).
- Mesma ideia de separação, mais leve, organizada por funcionalidade:

```
frontend/src/
├── app/            # Rotas, providers, layout base
├── features/       # Uma pasta por funcionalidade
│   ├── editor/
│   ├── game-selection/
│   └── settings/
├── shared/
│   ├── api/        # Cliente HTTP: único lugar que conversa com o backend
│   ├── ui/         # Componentes visuais reutilizáveis (Button, Card, Input)
│   └── styles/     # Tokens de cor e fontes
└── main.tsx
```

- Componentes não chamam `fetch` direto: usam funções de `shared/api`.
- A chave do Gemini fica só no navegador do usuário (localStorage, com opção de esquecer) e vai para a API em um header a cada requisição.
- Estilos com as variáveis CSS dos tokens da identidade visual.

### Rodar localmente
`docker compose up` deve subir frontend e backend com um comando só. Documentar no README.

## Segurança da chave (regra inegociável)

- A API **nunca** salva, persiste ou grava em log a chave do Gemini.
- Filtrar o header da chave de qualquer log, rastreamento ou mensagem de erro.
- Não devolver a chave em respostas.
- Deixar isso explícito no README e na tela de Configurações.

## Direitos de imagem dos jogos

- Logos e artes oficiais têm dono. Consultar a política de conteúdo de fãs de cada empresa (Valve, Riot etc.) antes de adicionar um jogo.
- Usar o logo do jogo **só** para identificar a opção na seleção.
- Gerar imagens "no estilo" do jogo; não copiar personagens, mapas ou artes oficiais nos modelos.
- README deve dizer que o projeto não é oficial, não é comercial e não tem vínculo com as empresas dos jogos.

## Qualidade

- Testes automatizados no backend com **xUnit**, um projeto de testes por camada:
  - `Domain.Tests` — regras de negócio das entidades, sem nenhum mock.
  - `Application.Tests` — casos de uso com as interfaces simuladas (mocks de `ITemplateRepository`, `IImageComposer`, `IAiImageGenerator`).
  - `Api.Tests` — testes de integração dos endpoints com `WebApplicationFactory`, com o Gemini simulado. Incluir um teste que garante que a chave não aparece nos logs.
- **GitHub Actions** rodando build e testes a cada push e pull request.
- Licença **MIT**.

## Ferramentas sugeridas no Claude Code

- Context7 — documentação atualizada de React e .NET.
- Frontend Design — visual menos genérico, alinhado à identidade acima.
- Playwright — testar a interface no navegador.
- GitHub — issues e pull requests.
- Opcional: Superpowers — planejar, testar e revisar.

## Ordem sugerida de implementação

1. Estrutura do monorepo, pasta `brand/` com o logo, licença MIT, README inicial e `docker-compose.yml`.
2. Backend: solution com os 4 projetos da Clean Architecture e as referências entre eles, projetos de teste, endpoint de saúde, Dockerfile e GitHub Actions.
3. Domain: entidades `GameTemplate`, `ArtFormat` e afins, com testes.
4. Frontend: projeto Vite, estrutura de pastas, tokens de cor e fontes, layout base e navegação entre as telas.
5. Tela de Configurações e envio da chave no header.
6. Application + Infrastructure: modelos por jogo (JSON) e composição de imagem com SkiaSharp, sem IA.
7. Editor com prévia e download em PNG.
8. Integração com o Gemini (`GeminiImageGenerator`).
9. Deploy (Cloudflare Pages + host do backend) e README com manual de uso.
