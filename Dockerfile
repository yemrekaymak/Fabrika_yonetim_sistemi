FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Sadece .csproj dosyasını kopyala ve restore et
# (Klasör adı kullanmadan direkt dosya adıyla)
COPY *.csproj ./
RUN dotnet restore

# Kalan tüm dosyaları kopyala ve derle
COPY . .
RUN dotnet publish -c Release -o /app/out

# Çalıştırma aşaması
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "FabrikaBackend.dll"]