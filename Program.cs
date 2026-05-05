using Microsoft.EntityFrameworkCore;
using Ambulancia_MIS.Data;
using Ambulancia_MIS.Services;

var builder = WebApplication.CreateBuilder(args);

// ==================== CONFIGURACIÓN PARA RAILWAY ====================
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

Console.WriteLine($"🚀 Iniciando en puerto: {port}");

// ==================== BASE DE DATOS ====================
// Railway usa DATABASE_URL (asignada automáticamente al agregar PostgreSQL)
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (string.IsNullOrEmpty(databaseUrl))
{
    databaseUrl = builder.Configuration.GetConnectionString("AmbulanciaContext");
    Console.WriteLine("⚠️ Usando ConnectionString desde appsettings.json");
}
else
{
    Console.WriteLine("✅ DATABASE_URL encontrada en variables de entorno");
    Console.WriteLine($"📡 URL parcial: {databaseUrl[..Math.Min(50, databaseUrl.Length)]}...");
}

// Convertir URL de Railway a formato Npgsql
string connectionString;
if (!string.IsNullOrEmpty(databaseUrl) && databaseUrl.Contains("postgresql://"))
{
    try
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true;";
        Console.WriteLine($"✅ Host: {uri.Host}, Database: {uri.AbsolutePath.TrimStart('/')}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error parsing DATABASE_URL: {ex.Message}");
        connectionString = databaseUrl;
    }
}
else if (!string.IsNullOrEmpty(databaseUrl))
{
    connectionString = databaseUrl;
    Console.WriteLine("⚠️ Usando connection string sin conversion (formato desconocido)");
}
else
{
    connectionString = "Host=localhost;Database=AmbulanciaDB;Username=postgres;Password=postgres";
    Console.WriteLine("⚠️ Usando connection string por defecto (local)");
}

builder.Services.AddDbContext<AmbulanciaContext>(options =>
    options.UseNpgsql(connectionString));

// ==================== SERVICIOS ====================
builder.Services.AddHttpClient();
builder.Services.AddScoped<IConexionExternaService, ConexionExternaService>();

// ==================== CORS ====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==================== MIGRACIONES Y DATOS SEMILLA ====================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AmbulanciaContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("🔄 Intentando conectar a PostgreSQL...");
        
        // Verificar conexión
        if (db.Database.CanConnect())
        {
            logger.LogInformation("✅ Conexión a PostgreSQL establecida.");
            
            // Ejecutar migraciones
            logger.LogInformation("🔄 Ejecutando migraciones...");
            db.Database.Migrate();
            logger.LogInformation("✅ Migraciones ejecutadas correctamente.");
            
            // Verificar datos
            var ambulanciasCount = db.Ambulancias.Count();
            var insumosCount = db.Insumos.Count();
            logger.LogInformation($"📊 Datos cargados: {ambulanciasCount} ambulancias, {insumosCount} insumos.");
        }
        else
        {
            logger.LogError("❌ No se pudo conectar a PostgreSQL");
        }
    }
    catch (Exception ex)
    {
        logger.LogError($"❌ Error al migrar BD: {ex.Message}");
        logger.LogError($"Stack trace: {ex.StackTrace}");
        // No detener la aplicación, seguir para que Swagger funcione
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
