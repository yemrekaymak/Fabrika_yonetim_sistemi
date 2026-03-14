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
            // Render ve localhost arasındaki URL uyumsuzluklarını önlemek için:
            ValidateIssuer = false, 
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // Key'in en az 16 karakter olduğundan emin olun
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "CokGizliAnahtar123!"))
        };
    });

// --- 3. CORS AYARI (CORS HATALARI İÇİN TAM ÇÖZÜM) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Tüm kaynaklara (localhost ve Render) izin ver
              .AllowAnyMethod()                   // GET, POST, PUT, DELETE, PATCH
              .AllowAnyHeader();                  // Authorization, Content-Type vb.
    });
});

// --- 4. DİĞER SERVİSLER ---
builder.Services.AddControllers().AddJsonOptions(o => {
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    // Frontend (gider_adi) ve Backend (GiderAdi) arasındaki büyük/küçük harf sorununu çözer:
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

// --- MIDDLEWARE SIRALAMASI (BU SIRA ÇOK KRİTİK!) ---
app.UseSwagger();
app.UseSwaggerUI(c => { 
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1"); 
    c.RoutePrefix = string.Empty; 
});

// 1. Rotalama
app.UseRouting(); 

// 2. CORS (Mutlaka Auth'dan önce gelmeli ki tarayıcı isteği reddetmesin)
app.UseCors("FrontendPolicy");

// 3. Yetkilendirme
app.UseAuthentication();
app.UseAuthorization();

// 4. Endpointler
app.MapControllers();

app.Run();