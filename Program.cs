using FabrikaBackend.Data;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Services;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVİS YAPILANDIRMALARI (builder.Build() öncesi her şey burada olmalı) ---

// CORS Ayarı (Arkadaşının hatasını çözen kısım burası)
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

// JSON ve Enum Ayarları
builder.Services.AddControllers()
    .AddJsonOptions(options => 
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Veritabanı Bağlantı Ayarı (Railway & SQLite Otomatik Seçim)
var rawConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
string? connectionString;

if (!string.IsNullOrEmpty(rawConnectionString) && (rawConnectionString.StartsWith("postgres://") || rawConnectionString.StartsWith("postgresql://")))
{
    var databaseUri = new Uri(rawConnectionString);
    var userInfo = databaseUri.UserInfo.Split(':');
    connectionString = $"Host={databaseUri.Host};Port={databaseUri.Port};Database={databaseUri.LocalPath.Substring(1)};" +
                       $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}
else
{
    connectionString = rawConnectionString ?? builder.Configuration.GetConnectionString("SqliteConnection") ?? "Data Source=fabrika.db";
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (connectionString.Contains("Host="))
        options.UseNpgsql(connectionString);
    else
        options.UseSqlite(connectionString);
});

// JWT Ayarları
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

builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fabrika API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Token girin: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Arka plan üretim takip servisini sisteme ekle
builder.Services.AddHostedService<ProductionTrackerService>();

// --- 2. UYGULAMA İNŞA ET (Build) ---
var app = builder.Build();

// --- 3. VERİTABANI OTOMASYONU (Migration & EnsureCreated) ---
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try 
    {
        context.Database.Migrate(); // Migrationları basar
        Console.WriteLine("--> Database hazır.");
    }
    catch { context.Database.EnsureCreated(); } // Migration yoksa direkt oluşturur
}

// --- 4. MIDDLEWARE SIRALAMASI (Sıralama çok önemli!) ---

app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fabrika API V1");
    c.RoutePrefix = string.Empty; 
});

// CORS Middleware'i Authentication'dan ÖNCE gelmeli
app.UseCors("HerkesGelsin"); 

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();