using FabrikaBackend.Data;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Services;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Render / Heroku: PORT env ile dinle (production'da zorunlu)
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// --- 1. VERİTABANI ---
// Lokalde verilerin kalıcı olması için DB her zaman proje klasöründe (göreli yol yerine sabit yol)
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "fabrika.db");
var connectionString = builder.Configuration.GetConnectionString("SqliteConnection") ?? $"Data Source={dbPath}";
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

// --- 2. AUTHENTICATION (401 HATALARINA SON) ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = false, 
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // AuthController'daki key ile birebir aynı olmalı!
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "CokGizliAnahtar123!"))
        };
    });

// --- 3. CORS AYARI (TARAYICI ENGELİNE SON) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("HerkesGelsin", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) 
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Content-Disposition"); 
    });
});

// --- 4. JSON VE DİĞER SERVİSLER ---
builder.Services.AddControllers().AddJsonOptions(o => {
    // Enum'ları string olarak döndür (örn: "Aktif" yerine "Active")
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    // ÖNEMLİ: Frontend büyük/küçük harf takılmasın diye PropertyNamingPolicy'yi null bıraktık
    o.JsonSerializerOptions.PropertyNamingPolicy = null; 
    // Döngüsel referansları engelle (Personel -> Departman -> Personel gibi)
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- 🛠️ OTOMATİK VERİTABANI GÜNCELLEME ---
using (var scope = app.Services.CreateScope()) {
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
    // Eski şemada UrunAdi kolonu yoksa ekle (500 hatasını önlemek için)
    try {
        var conn = context.Database.GetDbConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "PRAGMA table_info(Products)";
        var hasUrunAdi = false;
        using (var r = cmd.ExecuteReader()) {
            while (r.Read()) {
                var name = r.GetString(1);
                if (string.Equals(name, "UrunAdi", StringComparison.OrdinalIgnoreCase)) { hasUrunAdi = true; break; }
            }
        }
        if (!hasUrunAdi) {
            cmd.CommandText = "ALTER TABLE Products ADD COLUMN UrunAdi TEXT DEFAULT ''";
            cmd.ExecuteNonQuery();
        }
        var hasBirimUretimSuresiSaat = false;
        cmd.CommandText = "PRAGMA table_info(Products)";
        using (var r2 = cmd.ExecuteReader()) {
            while (r2.Read()) {
                var name = r2.GetString(1);
                if (string.Equals(name, "BirimUretimSuresiSaat", StringComparison.OrdinalIgnoreCase)) { hasBirimUretimSuresiSaat = true; break; }
            }
        }
        if (!hasBirimUretimSuresiSaat) {
            cmd.CommandText = "ALTER TABLE Products ADD COLUMN BirimUretimSuresiSaat REAL NULL";
            cmd.ExecuteNonQuery();
        }
        var hasSalePrice = false;
        cmd.CommandText = "PRAGMA table_info(Products)";
        using (var r3 = cmd.ExecuteReader()) {
            while (r3.Read()) {
                var name = r3.GetString(1);
                if (string.Equals(name, "SalePrice", StringComparison.OrdinalIgnoreCase)) { hasSalePrice = true; break; }
            }
        }
        if (!hasSalePrice) {
            cmd.CommandText = "ALTER TABLE Products ADD COLUMN SalePrice REAL NULL";
            cmd.ExecuteNonQuery();
        }
        conn.Close();
    } catch { /* Tablo yoksa veya kolon zaten varsa yoksay */ }
}

// --- MIDDLEWARE SIRALAMASI (BU SIRA HAYATİDİR) ---

app.UseSwagger();
app.UseSwaggerUI(c => { 
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1"); 
    c.RoutePrefix = string.Empty; 
});

app.UseRouting(); // 1. Önce yol bulunur.

app.UseCors("HerkesGelsin"); // 2. Sonra "kapıdan girebilir miyim?" (CORS) bakılır.

app.UseAuthentication(); // 3. "Kimsin?" (Token kontrolü)
app.UseAuthorization();  // 4. "Buraya girmeye yetkin var mı?"

app.MapControllers();

app.Run();