using ClientesAPI.Data;
using ClientesAPI.DTOs;
using ClientesAPI.Models;
using ClientesAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArchivosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IArchivoService _archivoService;

        public ArchivosController(ApplicationDbContext context, IArchivoService archivoService)
        {
            _context = context;
            _archivoService = archivoService;
        }

        // POST: api/Archivos/subir-multiple
        [HttpPost("subir-multiple")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ResponseDto>> SubirArchivosMultiples(
            string ciCliente,
            IFormFile archivoZip)
        {
            try
            {
                // Validar que el cliente exista
                var clienteExiste = await _context.Clientes.AnyAsync(c => c.CI == ciCliente);
                if (!clienteExiste)
                {
                    return NotFound(new ResponseDto
                    {
                        Success = false,
                        Message = "Cliente no encontrado"
                    });
                }

                // Validar que sea un archivo ZIP
                if (archivoZip == null || !archivoZip.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new ResponseDto
                    {
                        Success = false,
                        Message = "Debe proporcionar un archivo ZIP válido"
                    });
                }

                // Descomprimir y guardar archivos
                var rutasArchivos = await _archivoService.DescomprimirYGuardarAsync(archivoZip, ciCliente);

                // Registrar archivos en la base de datos
                var archivosGuardados = new List<ArchivoCliente>();
                foreach (var ruta in rutasArchivos)
                {
                    var fileInfo = new FileInfo(ruta);
                    var archivo = new ArchivoCliente
                    {
                        CICliente = ciCliente,
                        NombreArchivo = Path.GetFileName(ruta),
                        UrlArchivo = ruta,
                        FechaSubida = DateTime.Now,
                        TipoArchivo = fileInfo.Extension,
                        TamañoBytes = fileInfo.Length
                    };

                    _context.ArchivosClientes.Add(archivo);
                    archivosGuardados.Add(archivo);
                }

                await _context.SaveChangesAsync();

                return Ok(new ResponseDto
                {
                    Success = true,
                    Message = $"Se guardaron {archivosGuardados.Count} archivos exitosamente",
                    Data = archivosGuardados
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto
                {
                    Success = false,
                    Message = "Error al procesar archivos",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // GET: api/Archivos/cliente/{ci}
        [HttpGet("cliente/{ci}")]
        public async Task<ActionResult<ResponseDto>> ObtenerArchivosPorCliente(string ci)
        {
            try
            {
                var archivos = await _context.ArchivosClientes
                    .Where(a => a.CICliente == ci)
                    .OrderByDescending(a => a.FechaSubida)
                    .ToListAsync();

                return Ok(new ResponseDto
                {
                    Success = true,
                    Message = $"Se encontraron {archivos.Count} archivos",
                    Data = archivos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto
                {
                    Success = false,
                    Message = "Error al obtener archivos",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // GET: api/Archivos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDto>> ObtenerArchivo(int id)
        {
            try
            {
                var archivo = await _context.ArchivosClientes.FindAsync(id);

                if (archivo == null)
                {
                    return NotFound(new ResponseDto
                    {
                        Success = false,
                        Message = "Archivo no encontrado"
                    });
                }

                return Ok(new ResponseDto
                {
                    Success = true,
                    Data = archivo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto
                {
                    Success = false,
                    Message = "Error al obtener archivo",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // DELETE: api/Archivos/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseDto>> EliminarArchivo(int id)
        {
            try
            {
                var archivo = await _context.ArchivosClientes.FindAsync(id);

                if (archivo == null)
                {
                    return NotFound(new ResponseDto
                    {
                        Success = false,
                        Message = "Archivo no encontrado"
                    });
                }

                // Eliminar archivo físico
                if (System.IO.File.Exists(archivo.UrlArchivo))
                {
                    System.IO.File.Delete(archivo.UrlArchivo);
                }

                // Eliminar registro de la BD
                _context.ArchivosClientes.Remove(archivo);
                await _context.SaveChangesAsync();

                return Ok(new ResponseDto
                {
                    Success = true,
                    Message = "Archivo eliminado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto
                {
                    Success = false,
                    Message = "Error al eliminar archivo",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // GET: api/Archivos/descargar/{id}
        [HttpGet("descargar/{id}")]
        public async Task<IActionResult> DescargarArchivo(int id)
        {
            try
            {
                var archivo = await _context.ArchivosClientes.FindAsync(id);

                if (archivo == null)
                {
                    return NotFound(new ResponseDto
                    {
                        Success = false,
                        Message = "Archivo no encontrado"
                    });
                }

                if (!System.IO.File.Exists(archivo.UrlArchivo))
                {
                    return NotFound(new ResponseDto
                    {
                        Success = false,
                        Message = "El archivo físico no existe"
                    });
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(archivo.UrlArchivo, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                var contentType = archivo.TipoArchivo switch
                {
                    ".pdf" => "application/pdf",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".doc" => "application/msword",
                    ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    _ => "application/octet-stream"
                };

                return File(memory, contentType, archivo.NombreArchivo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto
                {
                    Success = false,
                    Message = "Error al descargar archivo",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}