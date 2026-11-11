<p align="center">
  <img src="brand/logo-bora-live.svg" alt="bora live" width="360">
</p>

<p align="center">
  Thumbnails, banners e posts de "tô ao vivo" para streamers, prontos em poucos cliques.
</p>

---

> 🚧 **Em construção.** O projeto está nas primeiras etapas. Este README vai crescer junto com o código.

## O que é

O **Bora Live** é uma plataforma open source que gera as artes que todo streamer precisa: thumbnail, banner de perfil, post de "tô ao vivo" e story. Tudo parte de modelos prontos por jogo, começando com CS2 e Valorant.

É feito para streamers e criadores pequenos, principalmente brasileiros, que precisam de visual constante e não querem (ou não têm tempo de) abrir um editor de imagem toda vez que vão ao ar.

### O que vai dar pra fazer

- Escolher o formato e o jogo, e partir de um modelo pronto.
- Colocar seu nome, o título da live e o seu logo.
- Gerar o fundo com IA (Gemini), usando a **sua própria chave**.
- Ver a prévia em tempo real e baixar a arte em PNG.

### Formatos

| Formato | Plataforma | Tamanho |
|---|---|---|
| Thumbnail | YouTube | 1280×720 |
| Banner de perfil | Twitch | 1200×480 |
| Post quadrado | Instagram / X | 1080×1080 |
| Story | Instagram | 1080×1920 |

## Traga sua própria chave

O Bora Live não paga API de IA. Para gerar imagens, você usa a sua chave do Gemini, que pode ser criada no [Google AI Studio](https://aistudio.google.com/).

**Sua chave fica com você:**

- Ela fica salva só no seu navegador, e você pode apagá-la quando quiser.
- Ela vai para a API apenas no momento de cada requisição.
- O servidor **nunca** salva, grava em log ou devolve a sua chave.

## Tecnologias

| Parte | Stack |
|---|---|
| Frontend | React, TypeScript, Vite e React Router, hospedado no Cloudflare Pages |
| Backend | C# e ASP.NET Core (.NET 10, Minimal APIs), com Clean Architecture |
| Imagem | SkiaSharp para a composição e Gemini para a geração com IA |
| Testes | xUnit |
| Infra | Docker e GitHub Actions |

## Estrutura do repositório

```
/
├── frontend/   # React + TypeScript + Vite
├── backend/    # Solution .NET (Domain, Application, Infrastructure, Api) e testes
├── brand/      # Logo e arquivos da marca
├── docker-compose.yml
└── LICENSE
```

## Rodar localmente

Você precisa do [Docker](https://www.docker.com/products/docker-desktop/) instalado.

```bash
git clone https://github.com/nicolas-daher/bora-live.git
cd bora-live
docker compose up --build
```

Com tudo no ar:

| Endereço | O que é |
|---|---|
| http://localhost:5173 | O Bora Live |
| http://localhost:8080/health | Verificação de saúde da API |
| http://localhost:8080/swagger | Documentação interativa da API (Swagger) |

### Sem Docker

Com o [SDK do .NET 10](https://dotnet.microsoft.com/download) e o [Node 22](https://nodejs.org/) instalados, use dois terminais.

Backend:

```bash
cd backend
dotnet test
dotnet run --project src/BoraLive.Api
```

Frontend, com recarga automática a cada alteração:

```bash
cd frontend
npm install
npm run dev
```

## Aviso

O Bora Live é um projeto **pessoal, não oficial e não comercial**. Ele **não tem vínculo** com Valve, Riot Games nem com nenhuma outra empresa dos jogos citados. Os nomes dos jogos pertencem aos seus donos e aparecem só em texto, para identificar os modelos. O projeto não usa logos, personagens, mapas nem artes oficiais.

## Licença

Distribuído sob a licença [MIT](LICENSE).
