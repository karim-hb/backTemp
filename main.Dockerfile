# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
USER root
EXPOSE 80


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Narije.Api/Narije.Api.csproj", "Narije.Api/"]
COPY ["Narije.Infrastructure/Narije.Infrastructure.csproj", "Narije.Infrastructure/"]
COPY ["Narije.Core/Narije.Core.csproj", "Narije.Core/"]
RUN dotnet restore "Narije.Api/Narije.Api.csproj"
COPY . .
WORKDIR "/src/Narije.Api"
RUN dotnet build "Narije.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Narije.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install LibreOffice (headless) and common fonts for proper Persian rendering
RUN apt-get update \
    && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
       libreoffice \
       fonts-dejavu \
       fonts-dejavu-core \
       fonts-dejavu-extra \
       fonts-liberation \
       fonts-noto \
       fonts-noto-cjk \
       fonts-noto-mono \
       fonts-noto-color-emoji \
       fonts-noto-unhinted \
       fonts-noto-ui-core \
       fonts-noto-ui-extra \
       fonts-noto-extra \
       fonts-freefont-ttf \
       ttf-mscorefonts-installer || true \
    && rm -rf /var/lib/apt/lists/*

RUN ln -s /data/ /app/images

ENTRYPOINT ["dotnet", "Narije.Api.dll"]