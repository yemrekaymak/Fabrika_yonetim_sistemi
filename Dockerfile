FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# GitHub'daki ana dizindeki her şeyi Docker içine kopyala
COPY . .

# Restore ve Publish işlemini direkt bu dizinde yap
RUN dotnet restore "FabrikaBackend.csproj"
RUN dotnet publish "FabrikaBackend.csproj" -c Release -o /app/out

# Çalıştırma aşaması
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "FabrikaBackend.dll"]