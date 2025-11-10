using ClientesAPI.Services;
using Microsoft.AspNetCore.Hosting;
using System.IO.Compression;

namespace ClientesAPI.Services
{
    public class ArchivoService : IArchivoService
    {
        private readonly string _rutaBase;
        private readonly ILogger<ArchivoService> _logger;

        public ArchivoService(IWebHostEnvironment environment, ILogger<ArchivoService> logger)
        {
            _rutaBase = Path.Combine(environment.ContentRootPath, "Uploads");
            _logger = logger;

            if (!Directory.Exists(_rutaBase))
            {
                Directory.CreateDirectory(_rutaBase);
            }
        }

        public async Task<string> GuardarArchivoAsync(IFormFile archivo, string ciCliente)
        {
            try
            {
                var carpetaCliente = Path.Combine(_rutaBase, ciCliente);
                if (!Directory.Exists(carpetaCliente))
                {
                    Directory.CreateDirectory(carpetaCliente);
                }

                var nombreArchivo = $"{Guid.NewGuid()}_{archivo.FileName}";
                var rutaCompleta = Path.Combine(carpetaCliente, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                _logger.LogInformation($"Archivo guardado: {rutaCompleta}");
                return rutaCompleta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar archivo");
                throw new Exception($"Error al guardar archivo: {ex.Message}");
            }
        }

        public async Task<List<string>> DescomprimirYGuardarAsync(IFormFile archivoZip, string ciCliente)
        {
            var rutasArchivos = new List<string>();

            try
            {
                var carpetaCliente = Path.Combine(_rutaBase, ciCliente);
                if (!Directory.Exists(carpetaCliente))
                {
                    Directory.CreateDirectory(carpetaCliente);
                }

                var carpetaTemporal = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(carpetaTemporal);

                var rutaZipTemporal = Path.Combine(carpetaTemporal, archivoZip.FileName);
                using (var stream = new FileStream(rutaZipTemporal, FileMode.Create))
                {
                    await archivoZip.CopyToAsync(stream);
                }

                ZipFile.ExtractToDirectory(rutaZipTemporal, carpetaTemporal);

                var archivosDescomprimidos = Directory.GetFiles(carpetaTemporal, "*.*", SearchOption.AllDirectories)
                    .Where(f => !f.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

                foreach (var archivoDescomprimido in archivosDescomprimidos)
                {
                    var nombreArchivo = Path.GetFileName(archivoDescomprimido);
                    var nombreUnico = $"{Guid.NewGuid()}_{nombreArchivo}";
                    var rutaDestino = Path.Combine(carpetaCliente, nombreUnico);

                    File.Copy(archivoDescomprimido, rutaDestino, true);
                    rutasArchivos.Add(rutaDestino);
                }

                Directory.Delete(carpetaTemporal, true);
                return rutasArchivos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descomprimir");
                throw new Exception($"Error al descomprimir: {ex.Message}");
            }
        }
    }
}