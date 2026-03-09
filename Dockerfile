# 1. Aşama: Build (SDK versiyonunu 9.0 yapıyoruz)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Her şeyi kopyala
COPY . .

# Restore ve Publish işlemlerini yap
RUN dotnet restore "FabrikaBackend.csproj"
RUN dotnet publish "FabrikaBackend.csproj" -c Release -o /app/out

# 2. Aşama: Runtime (Runtime versiyonunu da 9.0 yapıyoruz)
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "FabrikaBackend.dll"]