using FabrikaBackend.Data;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Services;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. VERİTABANI BAĞLANTISI (OTOMATİK SEÇİM & FORMAT DÖNÜŞTÜRME) ---
var rawConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
string? connectionString;

if (!string.IsNullOrEmpty(rawConnectionString) && (rawConnectionString.StartsWith("postgres://") || rawConnectionString.StartsWith("postgresql://")))
{
    // Railway'in "postgres://" veya "postgresql://" formatını Npgsql'in anlayacağı "Host=..." formatına çeviriyoruz
    var databaseUri = new Uri(rawConnectionString);
    var userInfo = databaseUri.UserInfo.Split(':');
    
    connectionString = $"Host={databaseUri.Host};Port={databaseUri.Port};Database={databaseUri.LocalPath.Substring(1)};" +
                       $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    Console.WriteLine($"--> [DB] PostgreSQL bağlantısı kullanılıyor: {connectionString.Replace(userInfo[1], "***")}");
}
else
{
    // Yereldeysen appsettings.json'daki SqliteConnection'ı kullan
    connectionString = rawConnectionString ?? builder.Configuration.GetConnectionString("SqliteConnection") ?? "Data Source=fabrika.db";
    Console.WriteLine($"--> [DB] SQLite bağlantısı kullanılıyor: {connectionString}");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (connectionString.Contains("Host="))
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

// --- 2. AUTHENTICATION & JWT YAPILANDIRMASI ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "CokGizliAnahtar123!"))
        };
    });

// --- 3. JSON VE ENUM AYARLARI ---
builder.Services.AddControllers()
    .AddJsonOptions(options => 
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- 4. CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("HerkesGelsin", policyBuilder =>
    {
        policyBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// --- 5. OTONOM TAKİP SERVİSİ ---
// builder.Services.AddHostedService<ProductionTrackerService>(); // Tetikleyici mantık kaldırıldı

var app = builder.Build();

// --- 🛠️ VERİTABANI OTOMASYONU ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        
        Console.WriteLine("--> [DB] Migration'lar uygulanıyor...");
        context.Database.Migrate();
        Console.WriteLine("--> [BAŞARILI] Tüm migration'lar uygulandı ve tablolar oluşturuldu.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> [HATA] Veritabanı hazırlığı sırasında hata: {ex.Message}");
        Console.WriteLine($"--> [DETAY] {ex.InnerException?.Message}");
    }
}

// --- 6. MIDDLEWARE SIRALAMASI ---
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fabrika API V1");
    c.RoutePrefix = string.Empty; 
});

app.UseCors("HerkesGelsin"); 
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();

app.Run();