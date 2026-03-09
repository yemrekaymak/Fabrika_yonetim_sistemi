FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Mevcut dizindeki her şeyi kopyala
COPY . .

# Restore ve Publish işlemlerini tek satırda yapalım (Dosya yolu hatasını önler)
RUN dotnet publish "FabrikaBackend.csproj" -c Release -o /app/out

# Runtime aşaması
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "FabrikaBackend.dll"]