FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar y restaurar
COPY ["Ambulancia_MIS.csproj", "."]
RUN dotnet restore "./Ambulancia_MIS.csproj"

# Copiar todo y publicar
COPY . .
RUN dotnet publish "./Ambulancia_MIS.csproj" -c Release -o /app/publish

# Imagen final - más ligera
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Ambulancia_MIS.dll"]
