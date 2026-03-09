FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Her şeyi içeri al
COPY . .

# Klasördeki .csproj dosyasını Docker kendisi bulsun ve derlesin
RUN dotnet publish *.csproj -c Release -o /app/out

# Runtime aşaması
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# DLL ismini de joker karakterle kontrol edelim
ENTRYPOINT ["sh", "-c", "dotnet $(ls *.dll | head -n 1)"]