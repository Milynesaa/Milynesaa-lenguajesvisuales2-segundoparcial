using ClientesAPI.Data;
using ClientesAPI.Models;
using System.Text;

namespace ClientesAPI.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            // ⚡ Evitar interferir con Swagger y archivos estáticos
            if (context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.StartsWithSegments("/favicon") ||
                context.Request.Path.StartsWithSegments("/index.html"))
            {
                await _next(context);
                return;
            }

            var requestBody = string.Empty;
            var responseBody = string.Empty;
            var originalBodyStream = context.Response.Body;

            try
            {
                // Capturar el request body solo si existe
                if (context.Request.ContentLength > 0 && context.Request.ContentType != null
                    && !context.Request.ContentType.Contains("multipart/form-data"))
                {
                    context.Request.EnableBuffering();
                    using (var reader = new StreamReader(
                        context.Request.Body,
                        Encoding.UTF8,
                        detectEncodingFromByteOrderMarks: false,
                        bufferSize: 1024,
                        leaveOpen: true))
                    {
                        requestBody = await reader.ReadToEndAsync();
                        context.Request.Body.Position = 0;
                    }
                }

                // Capturar el response body
                using (var responseBodyStream = new MemoryStream())
                {
                    context.Response.Body = responseBodyStream;

                    // Ejecutar el siguiente middleware
                    await _next(context);

                    // Leer la respuesta
                    responseBodyStream.Seek(0, SeekOrigin.Begin);
                    responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
                    responseBodyStream.Seek(0, SeekOrigin.Begin);

                    // Copiar la respuesta al stream original
                    await responseBodyStream.CopyToAsync(originalBodyStream);
                }

                // Registrar log solo para errores 4xx y 5xx
                if (context.Response.StatusCode >= 400)
                {
                    await RegistrarLog(dbContext, context, requestBody, responseBody, "ERROR", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la aplicación");

                // Registrar log de error
                await RegistrarLog(dbContext, context, requestBody, string.Empty, "ERROR", ex.Message);

                // Restaurar el stream original
                context.Response.Body = originalBodyStream;

                // Enviar respuesta de error
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Error interno del servidor 💥",
                    error = ex.Message
                });
            }
        }

        private async Task RegistrarLog(
            ApplicationDbContext dbContext,
            HttpContext context,
            string requestBody,
            string responseBody,
            string tipoLog,
            string? detalle)
        {
            try
            {
                var log = new LogApi
                {
                    FechaHora = DateTime.Now,
                    TipoLog = tipoLog,
                    RequestBody = requestBody?.Length > 4000 ? requestBody.Substring(0, 4000) : requestBody,
                    ResponseBody = responseBody?.Length > 4000 ? responseBody.Substring(0, 4000) : responseBody,
                    UrlEndpoint = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}",
                    MetodoHttp = context.Request.Method,
                    DireccionIp = context.Connection.RemoteIpAddress?.ToString(),
                    Detalle = detalle,
                    CodigoEstado = context.Response.StatusCode
                };

                dbContext.LogsApi.Add(log);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar log en la base de datos");
            }
        }
    }
}
