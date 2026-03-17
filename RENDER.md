# Render’a Yükleme

## 1. PostgreSQL Veritabanı Oluştur (Render'da)

1. **Render Dashboard** → **PostgreSQL** → **New PostgreSQL**
2. Veritabanı oluştur (örn: "fabrika-db")
3. Render'dan sağladığı **DATABASE_URL** environment variable'ı otomatik olarak set edilir
   - Format: `postgresql://user:password@host:port/database`
   - **Program.cs** bu URL'i otomatik okur ve Npgsql uyumlu format'a dönüştürür

## 2. Backend (Web Service) Deploy Et

1. **Render** → **New** → **Web Service** → Repo'yu bağla
2. **Build Command:** `dotnet restore && dotnet publish -c Release -o out`
3. **Start Command:** `./out/FabrikaBackend` (veya `dotnet out/FabrikaBackend.dll`)
4. **Environment Variables:**
   - `ASPNETCORE_ENVIRONMENT` = `Production`
   - `DATABASE_URL` = Render PostgreSQL'den otomatik (manuel set etmeye gerek yok)
   - `Jwt__Key` = Üretim için güçlü bir key (isteğe bağlı)
   - `PORT` = Render tarafından otomatik verilir

## 3. Migration Çalıştır (İlk Deploy)

Backend ilk çalıştığında **EnsureCreated()** otomatik schema oluşturur. Migration'ları el ile çalıştırmanıza gerek yok.

---

## Frontend (Static Site veya Web Service)

1. **Build Command:** `npm install && npm run build`
2. **Environment (build sırasında):**
   - `REACT_APP_API_BASE_URL` = Backend’in tam URL’i (örn. `https://fabrika-backend-xxxx.onrender.com`)
3. Backend’i önce deploy edin; aldığınız URL’i frontend’de `REACT_APP_API_BASE_URL` olarak verin.

---

## Özet

- Backend’de `PORT` desteği eklendi; Render’da sorunsuz dinler.
- Frontend’de production build’de `REACT_APP_API_BASE_URL` mutlaka backend URL’i olmalı.
- SQLite kalıcı değil; demo için çalışır, ciddi kullanımda PostgreSQL/disk planı yapın.
