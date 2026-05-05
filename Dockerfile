FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Instalar dotnet-ef globalmente
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copiar y restaurar
COPY ["Ambulancia_MIS.csproj", "."]
RUN dotnet restore "./Ambulancia_MIS.csproj"

# Copiar todo y publicar
COPY . .
RUN dotnet publish "./Ambulancia_MIS.csproj" -c Release -o /app/publish

# Imagen final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Instalar dotnet-ef también en la imagen final
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Ambulancia_MIS.dll"]
