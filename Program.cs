using Microsoft.EntityFrameworkCore;
using Ambulancia_MIS.Data;
using Ambulancia_MIS.Services;

var builder = WebApplication.CreateBuilder(args);

// ==================== CONFIGURACIÓN PARA RAILWAY ====================
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ==================== BASE DE DATOS ====================
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
                       ?? builder.Configuration.GetConnectionString("AmbulanciaContext");

// Convertir URL de Railway a formato Npgsql
if (!string.IsNullOrEmpty(connectionString) && connectionString.Contains("postgresql://"))
{
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true;";
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

// ==================== MIGRACIONES Y DATOS SEMILLA (FORZADO) ====================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AmbulanciaContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("🔄 Intentando conectar a PostgreSQL...");
        
        // Intentar conectar con reintentos
        for (int i = 0; i < 10; i++)
        {
            try
            {
                if (db.Database.CanConnect())
                {
                    logger.LogInformation("✅ Conexión a PostgreSQL establecida.");
                    break;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning($"⚠️ Intento {i + 1}/10 falló: {ex.Message}");
                await Task.Delay(3000);
            }
        }
        
        // Ejecutar migraciones
        logger.LogInformation("🔄 Ejecutando migraciones...");
        db.Database.Migrate();
        logger.LogInformation("✅ Migraciones ejecutadas correctamente.");
        
        // Verificar si hay datos semilla (opcional)
        var tieneAmbulancias = db.Ambulancias.Any();
        if (!tieneAmbulancias)
        {
            logger.LogInformation("⚠️ No hay datos en la tabla Ambulancias. Verificar que los HasData() se ejecutaron.");
        }
        else
        {
            logger.LogInformation($"✅ Datos cargados: {db.Ambulancias.Count()} ambulancias, {db.Insumos.Count()} insumos.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError($"❌ Error al migrar BD: {ex.Message}");
        logger.LogError($"Stack trace: {ex.StackTrace}");
        // No detener la aplicación
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
