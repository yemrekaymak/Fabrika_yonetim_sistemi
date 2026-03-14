# 1. Derleme Aşaması
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyasını kopyala ve restore et
# Eğer .csproj dosyan bir alt klasördeyse yolu "KlasörAdı/*.csproj" yapmalısın
COPY *.csproj ./
RUN dotnet restore

# Kalan her şeyi kopyala ve yayınla
COPY . .
RUN dotnet publish -c Release -o /app/out

# 2. Çalıştırma Aşaması
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
# Build aşamasında /app/out klasörüne attığımız dosyaları buraya alıyoruz
COPY --from=build /app/out .

# Render/Railway port ayarı (Genelde 10000 kullanılır ama dinamik olması iyidir)
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "FabrikaBackend.dll"]