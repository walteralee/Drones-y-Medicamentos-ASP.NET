# Imagen de Drones y Medicamentos: API ASP.NET 8 que también sirve la web
# Uso:  docker compose up -d   y abrir http://localhost:5212

# 1) Compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY backend/DronesYMedicamentos.Api.csproj backend/
RUN dotnet restore backend/DronesYMedicamentos.Api.csproj
COPY backend/ backend/
RUN dotnet publish backend/DronesYMedicamentos.Api.csproj -c Release -o /app --no-restore

# 2) Ejecutar
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
COPY frontend/ wwwroot/

# Escucha en el puerto 8080 del contenedor y guarda la base de datos en /app/datos (volumen)
ENV ASPNETCORE_URLS=http://+:8080 \
    AbrirNavegador=false
EXPOSE 8080
VOLUME /app/datos

ENTRYPOINT ["dotnet", "DronesYMedicamentos.Api.dll"]
