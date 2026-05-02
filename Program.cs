using FabrikaBackend.Data;
using FabrikaBackend.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var dbProvider = (Environment.GetEnvironmentVariable("DB_PROVIDER")
    ?? builder.Configuration["Database:Provider"]
    ?? "sqlite").Trim().ToLowerInvariant();

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var sqlitePath = Path.Combine(builder.Environment.ContentRootPath, "fabrika.db");
var sqliteConnectionString = builder.Configuration.GetConnectionString("SqliteConnection")
    ?? $"Data Source={sqlitePath}";
var dataProtectionPath = Path.Combine(builder.Environment.ContentRootPath, ".keys");

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    dbProvider = "postgres";
}

if (dbProvider == "postgres")
{
    var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");

    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        var dbUri = new Uri(databaseUrl);
        var userInfo = dbUri.UserInfo.Split(':', 2);
        connectionString =
            $"Host={dbUri.Host};Port={dbUri.Port};Username={userInfo[0]};Password={userInfo[1]};Database={dbUri.LocalPath.TrimStart('/')};SSL Mode=Require;Trust Server Certificate=true;";
    }

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("PostgreSQL için bağlantı bilgisi bulunamadı.");
    }

    builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(sqliteConnectionString));
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "CokGizliAnahtar123!"))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("HerkesGelsin", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Content-Disposition");
    });
});

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    o.JsonSerializerOptions.PropertyNamingPolicy = null;
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<AiIntegrationService>();
builder.Services.AddHostedService<ProductionTrackerService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

    try
    {
        if (dbProvider == "postgres")
        {
            context.Database.Migrate();
        }
        else
        {
            context.Database.EnsureCreated();
        }

        EnsureLegacySchema(context);
        Console.WriteLine($"Veritabanı hazır. Provider: {dbProvider}");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Veritabanı hazırlanırken hata oluştu: " + ex.Message);
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1");
    c.RoutePrefix = string.Empty;
});

app.UseRouting();
app.UseCors("HerkesGelsin");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static void EnsureLegacySchema(AppDbContext context)
{
    if (context.Database.IsSqlite())
    {
        EnsureSqliteColumn(
            context,
            tableName: "Personnels",
            columnName: "Department",
            alterSql: "ALTER TABLE \"Personnels\" ADD COLUMN \"Department\" TEXT NULL;"
        );
        EnsureSqliteColumn(
            context,
            tableName: "Personnels",
            columnName: "PersonelCode",
            alterSql: "ALTER TABLE \"Personnels\" ADD COLUMN \"PersonelCode\" TEXT NULL;"
        );
        EnsureSqliteColumn(
            context,
            tableName: "Personnels",
            columnName: "IsActive",
            alterSql: "ALTER TABLE \"Personnels\" ADD COLUMN \"IsActive\" INTEGER NULL;"
        );
        EnsureSqliteColumn(
            context,
            tableName: "Personnels",
            columnName: "CreatedAt",
            alterSql: "ALTER TABLE \"Personnels\" ADD COLUMN \"CreatedAt\" TEXT NULL;"
        );
        EnsureSqliteColumn(
            context,
            tableName: "Products",
            columnName: "GunlukUretim",
            alterSql: "ALTER TABLE \"Products\" ADD COLUMN \"GunlukUretim\" INTEGER NULL;"
        );
        EnsureSqliteColumn(
            context,
            tableName: "Orders",
            columnName: "MarginPercent",
            alterSql: "ALTER TABLE \"Orders\" ADD COLUMN \"MarginPercent\" REAL NULL;"
        );
        EnsureSqliteColumn(
            context,
            tableName: "Orders",
            columnName: "DeliveryDate",
            alterSql: "ALTER TABLE \"Orders\" ADD COLUMN \"DeliveryDate\" TEXT NULL;"
        );
        EnsureSqliteColumn(
            context,
            tableName: "Orders",
            columnName: "Notes",
            alterSql: "ALTER TABLE \"Orders\" ADD COLUMN \"Notes\" TEXT NULL;"
        );
        context.Database.ExecuteSqlRaw("UPDATE \"Personnels\" SET \"Department\" = COALESCE(\"Department\", '');");
        context.Database.ExecuteSqlRaw("UPDATE \"Personnels\" SET \"PersonelCode\" = COALESCE(\"PersonelCode\", '');");
        context.Database.ExecuteSqlRaw("UPDATE \"Personnels\" SET \"IsActive\" = COALESCE(\"IsActive\", 1);");
        context.Database.ExecuteSqlRaw("UPDATE \"Personnels\" SET \"CreatedAt\" = COALESCE(\"CreatedAt\", CURRENT_TIMESTAMP);");
        context.Database.ExecuteSqlRaw("UPDATE \"Products\" SET \"GunlukUretim\" = COALESCE(\"GunlukUretim\", 0);");
        context.Database.ExecuteSqlRaw("UPDATE \"Orders\" SET \"MarginPercent\" = COALESCE(\"MarginPercent\", 0);");
        context.Database.ExecuteSqlRaw("UPDATE \"Orders\" SET \"Notes\" = COALESCE(\"Notes\", '');");
        return;
    }

    if (context.Database.IsNpgsql())
    {
        context.Database.ExecuteSqlRaw(
            "ALTER TABLE \"Personnels\" ADD COLUMN IF NOT EXISTS \"Department\" text NOT NULL DEFAULT '';"
        );
        context.Database.ExecuteSqlRaw(
            "ALTER TABLE \"Personnels\" ADD COLUMN IF NOT EXISTS \"PersonelCode\" text NOT NULL DEFAULT '';"
        );
        context.Database.ExecuteSqlRaw(
            "ALTER TABLE \"Personnels\" ADD COLUMN IF NOT EXISTS \"IsActive\" boolean NOT NULL DEFAULT true;"
        );
        context.Database.ExecuteSqlRaw(
            "ALTER TABLE \"Personnels\" ADD COLUMN IF NOT EXISTS \"CreatedAt\" timestamp with time zone NOT NULL DEFAULT now();"
        );
        context.Database.ExecuteSqlRaw(
            "ALTER TABLE \"Products\" ADD COLUMN IF NOT EXISTS \"GunlukUretim\" integer NOT NULL DEFAULT 0;"
        );
        context.Database.ExecuteSqlRaw(
            "ALTER TABLE \"Orders\" ADD COLUMN IF NOT EXISTS \"MarginPercent\" double precision NOT NULL DEFAULT 0;"
        );
        context.Database.ExecuteSqlRaw(
            "ALTER TABLE \"Orders\" ADD COLUMN IF NOT EXISTS \"DeliveryDate\" timestamp with time zone NULL;"
        );
        context.Database.ExecuteSqlRaw(
            "ALTER TABLE \"Orders\" ADD COLUMN IF NOT EXISTS \"Notes\" text NOT NULL DEFAULT '';"
        );
    }
}

static void EnsureSqliteColumn(AppDbContext context, string tableName, string columnName, string alterSql)
{
    var connection = context.Database.GetDbConnection();
    var shouldClose = connection.State != ConnectionState.Open;

    if (shouldClose)
        connection.Open();

    try
    {
        using var inspectCommand = connection.CreateCommand();
        inspectCommand.CommandText = $"PRAGMA table_info(\"{tableName}\");";

        using var reader = inspectCommand.ExecuteReader();
        while (reader.Read())
        {
            if (string.Equals(reader["name"]?.ToString(), columnName, StringComparison.OrdinalIgnoreCase))
                return;
        }
        reader.Close();

        using var alterCommand = connection.CreateCommand();
        alterCommand.CommandText = alterSql;
        alterCommand.ExecuteNonQuery();
    }
    finally
    {
        if (shouldClose)
            connection.Close();
    }
}
