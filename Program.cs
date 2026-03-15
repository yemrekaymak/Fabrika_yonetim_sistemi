using FabrikaBackend.Data;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Services;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. VERİTABANI ---
var connectionString = builder.Configuration.GetConnectionString("SqliteConnection") ?? "Data Source=fabrika.db";
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

// --- 2. AUTHENTICATION (401 HATALARI İÇİN KALICI ÇÖZÜM) ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = false, 
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "CokGizliAnahtar123!"))
        };
    });

// --- 3. CORS AYARI (HAYAT KURTARAN DÜZENLEME) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("HerkesGelsin", policy =>
    {
        // Bu ayar tarayıcının "Preflight" isteğine yeşil ışık yakar
        policy.SetIsOriginAllowed(origin => true) 
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Content-Disposition"); // Bazı tarayıcılar için ek güvenlik başlığı
    });
});

// --- 4. DİĞER SERVİSLER ---
builder.Services.AddControllers().AddJsonOptions(o => {
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    o.JsonSerializerOptions.PropertyNamingPolicy = null; 
});
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- 🛠️ OTOMATİK VERİTABANI OLUŞTURMA ---
using (var scope = app.Services.CreateScope()) {
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

// --- MIDDLEWARE SIRALAMASI (MÜHENDİS DOKUNUŞU) ---

// 1. Swagger (Her zaman en üstte olabilir)
app.UseSwagger();
app.UseSwaggerUI(c => { 
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1"); 
    c.RoutePrefix = string.Empty; 
});

// 2. Rotalama
app.UseRouting(); 

// 3. CORS (Mevzu burası! Auth ve Authorization'dan MUTLAKA önce gelmeli)
app.UseCors("HerkesGelsin");

// 4. Güvenlik Katmanları
app.UseAuthentication();
app.UseAuthorization();

// 5. Endpointler
app.MapControllers();

app.Run();