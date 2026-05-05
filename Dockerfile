# Consulte https://aka.ms/customizecontainer para aprender a personalizar su contenedor de depuración

# Esta fase se usa cuando se ejecuta desde VS en modo rápido
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Esta fase se usa para compilar el proyecto de servicio
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 🔧 CORREGIDO: El csproj está en la raíz, no en subcarpeta
COPY ["Ambulancia_MIS.csproj", "."]
RUN dotnet restore "./Ambulancia_MIS.csproj"

# Copiar todo el resto de archivos
COPY . .

# Compilar
RUN dotnet build "./Ambulancia_MIS.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicar
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Ambulancia_MIS.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Fase final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configurar puerto para Railway
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Ambulancia_MIS.dll"]
