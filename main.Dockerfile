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

RUN ln -s /data/ /app/images

ENTRYPOINT ["dotnet", "Narije.Api.dll"]