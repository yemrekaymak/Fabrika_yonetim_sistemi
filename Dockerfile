# Build aşaması için .NET 9 SDK kullan
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Proje dosyasını kopyala ve restore et
COPY *.csproj ./
RUN dotnet restore

# Diğer dosyaları kopyala ve yayınla
COPY . .
# DÜZELTİLEN SATIR: Proje adını açıkça belirtiyoruz
RUN dotnet publish "FabrikaBackend.csproj" -c Release -o /app/publish

# Çalışma aşaması (Runtime) için .NET 9 ASP.NET imajı kullan
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FabrikaBackend.dll"]