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

// --- 2. AUTHENTICATION ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "CokGizliAnahtar123!"))
        };
    });

// --- 3. CORS AYARI (HAYAT KURTARAN DÜZENLEME) ---
// Render'da yayınladığın için originleri daha esnek tutmalıyız
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.AllowAnyOrigin()    // Render ve Localhost arasındaki bariyeri kaldırır
              .AllowAnyMethod()    // GET, POST, PUT, DELETE hepsine izin verir
              .AllowAnyHeader();   // Authorization header'ına izin verir
        // NOT: AllowAnyOrigin varken AllowCredentials() kullanılmaz, o yüzden sildim.
    });
});

// --- 4. DİĞER SERVİSLER ---
builder.Services.AddControllers().AddJsonOptions(o => {
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    // JSON isimlendirmesinde frontend ile çakışma olmaması için:
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

// --- MIDDLEWARE SIRALAMASI ---
app.UseSwagger();
app.UseSwaggerUI(c => { 
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1"); 
    c.RoutePrefix = string.Empty; 
});

// Sıralama Kritiktir:
app.UseRouting(); // Rotaları belirle

app.UseCors("FrontendPolicy"); // CORS her zaman Auth'dan önce gelmeli!

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();