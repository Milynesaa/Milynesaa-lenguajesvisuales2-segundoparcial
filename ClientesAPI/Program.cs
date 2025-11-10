using ClientesAPI.Data;
using ClientesAPI.Middleware;
using ClientesAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// DbContext con resiliencia ante errores transitorios
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

// Servicios
builder.Services.AddScoped<IArchivoService, ArchivoService>();

// Controladores
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ClientesAPI",
        Version = "v1",
        Description = "API para gestión de clientes y sus archivos 📁"
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 🔥 Aplicar migraciones automáticamente al iniciar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate(); // Aplica migraciones pendientes
        Console.WriteLine("✅ Base de datos actualizada correctamente");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Error al aplicar migraciones: {Message}", ex.Message);
    }
}

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ClientesAPI v1");
    c.RoutePrefix = ""; // ⚡ Abre Swagger en https://localhost:7296/
});

// CORS
app.UseCors("AllowAll");

// Middleware de errores
app.UseMiddleware<ErrorHandlingMiddleware>();

// Autorización
app.UseAuthorization();

// Controladores
app.MapControllers();

// 🔍 Endpoint de diagnóstico (temporal - eliminar en producción)
app.MapGet("/api/test-connection", async (ApplicationDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();

        if (canConnect)
        {
            var clientesCount = await db.Clientes.CountAsync();
            var logsCount = await db.LogsApi.CountAsync();

            return Results.Ok(new
            {
                success = true,
                message = "✅ Conexión exitosa a LocalDB",
                database = "ClientesDB",
                clientes = clientesCount,
                logs = logsCount
            });
        }
        else
        {
            return Results.Problem("❌ No se puede conectar a LocalDB");
        }
    }
    catch (Exception ex)
    {
        return Results.Problem(new ProblemDetails
        {
            Title = "Error de conexión a LocalDB",
            Detail = ex.Message,
            Status = 500,
            Extensions =
            {
                ["innerException"] = ex.InnerException?.Message
            }
        });
    }
});

// Ejecutar app
app.Run();