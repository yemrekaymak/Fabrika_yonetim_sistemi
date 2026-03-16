# Render’a Yükleme

## Backend (Web Service)

1. **Render** → New → Web Service → Repo’yu bağla, root’u backend klasörüne ayarla (veya backend’i tek repo yap).
2. **Build Command:** `dotnet restore && dotnet publish -c Release -o out`
3. **Start Command:** `./out/FabrikaBackend` (veya `dotnet out/FabrikaBackend.dll`)
4. **Environment:**
   - `ASPNETCORE_ENVIRONMENT` = `Production`
   - `Jwt__Key` = Üretim için güçlü bir key (isteğe bağlı; yoksa appsettings’teki kullanılır.)
   - `PORT` Render tarafından otomatik verilir; uygulama bu portu dinler.

**Not:** SQLite kullanıyorsunuz. Render’da disk geçicidir; servis her yeniden deploy/restart’ta veritabanı sıfırlanabilir. Kalıcı veri için Render Disk veya harici bir DB (örn. PostgreSQL) kullanın.

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
