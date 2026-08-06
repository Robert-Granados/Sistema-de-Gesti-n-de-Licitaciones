FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["src/Licitaciones.Domain/Licitaciones.Domain.csproj", "src/Licitaciones.Domain/"]
COPY ["src/Licitaciones.Application/Licitaciones.Application.csproj", "src/Licitaciones.Application/"]
COPY ["src/Licitaciones.Infrastructure/Licitaciones.Infrastructure.csproj", "src/Licitaciones.Infrastructure/"]
COPY ["src/Licitaciones.Api/Licitaciones.Api.csproj", "src/Licitaciones.Api/"]
COPY ["src/Licitaciones.Web/Licitaciones.Web.csproj", "src/Licitaciones.Web/"]
RUN dotnet restore "src/Licitaciones.Web/Licitaciones.Web.csproj"

COPY src/Licitaciones.Domain/ src/Licitaciones.Domain/
COPY src/Licitaciones.Application/ src/Licitaciones.Application/
COPY src/Licitaciones.Infrastructure/ src/Licitaciones.Infrastructure/
COPY src/Licitaciones.Api/ src/Licitaciones.Api/
COPY src/Licitaciones.Web/ src/Licitaciones.Web/
RUN dotnet publish "src/Licitaciones.Web/Licitaciones.Web.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Licitaciones.Web.dll"]
