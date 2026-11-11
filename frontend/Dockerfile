# Imagem de DESENVOLVIMENTO: roda o servidor do Vite.
# Em produção o frontend vira arquivos estáticos no Cloudflare Pages (etapa 9),
# então não existe uma imagem de produção dele.
FROM node:22-alpine
WORKDIR /app

# Copia só os manifestos primeiro: a camada do "npm ci" é reaproveitada
# enquanto as dependências não mudarem.
COPY package.json package-lock.json ./
# "npm ci" instala exatamente as versões do package-lock.json.
RUN npm ci

COPY . .

EXPOSE 5173
CMD ["npm", "run", "dev"]
