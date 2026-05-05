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

// ==================== MIGRACIONES AUTOMÁTICAS AL INICIAR ====================
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AmbulanciaContext>();
        db.Database.Migrate();
        Console.WriteLine("Base de datos migrada correctamente");
    }
    catch (Exception ex)
    {
        Console.WriteLine($" Error al migrar BD: {ex.Message}");
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();