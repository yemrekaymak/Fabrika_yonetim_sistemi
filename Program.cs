using FabrikaBackend.Data;
using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Services;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. VERİTABANI ---
// SQLite bağlantısını garantiye alıyoruz
var connectionString = builder.Configuration.GetConnectionString("SqliteConnection") ?? "Data Source=fabrika.db";
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
    // Modelden bir alan sildiysen (Department gibi), DB'nin güncellenmesini sağlar
    context.Database.EnsureCreated();
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

app.MapControllers(); // 5. Ve aksiyon!

app.Run();