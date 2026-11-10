# ---------- Estágio 1: build ----------
# Imagem com o SDK completo: só é usada para compilar.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Copia só os .csproj primeiro e restaura os pacotes.
# Assim o Docker reaproveita essa camada enquanto as dependências não mudarem,
# e um build depois de alterar só o código fica bem mais rápido.
COPY src/BoraLive.Domain/BoraLive.Domain.csproj src/BoraLive.Domain/
COPY src/BoraLive.Application/BoraLive.Application.csproj src/BoraLive.Application/
COPY src/BoraLive.Infrastructure/BoraLive.Infrastructure.csproj src/BoraLive.Infrastructure/
COPY src/BoraLive.Api/BoraLive.Api.csproj src/BoraLive.Api/
RUN dotnet restore src/BoraLive.Api/BoraLive.Api.csproj

# Agora copia o código e publica em modo Release.
COPY src/ src/
RUN dotnet publish src/BoraLive.Api/BoraLive.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# ---------- Estágio 2: runtime ----------
# Imagem final só com o runtime do ASP.NET Core: menor e sem ferramentas de build.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Roda com o usuário sem privilégios que já vem na imagem, e não como root.
USER $APP_UID

# Porta padrão das imagens do ASP.NET Core.
EXPOSE 8080

ENTRYPOINT ["dotnet", "BoraLive.Api.dll"]
