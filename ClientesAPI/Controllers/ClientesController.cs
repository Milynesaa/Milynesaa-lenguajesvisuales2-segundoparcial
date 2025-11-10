using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClientesAPI.Data;
using ClientesAPI.Models;
using ClientesAPI.DTOs;
using ClientesAPI.Services;

namespace ClientesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IArchivoService _archivoService;

        public ClientesController(ApplicationDbContext context, IArchivoService archivoService)
        {
            _context = context;
            _archivoService = archivoService;
        }

        // POST: api/Clientes
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ResponseDto>> RegistrarCliente(
            [FromForm] string CI,
            [FromForm] string Nombres,
            [FromForm] string Direccion,
            [FromForm] string Telefono,
            IFormFile? fotoCasa1,
            IFormFile? fotoCasa2,
            IFormFile? fotoCasa3)
        {
            try
            {
                // Validar si el cliente ya existe
                if (await _context.Clientes.AnyAsync(c => c.CI == CI))
                {
                    return BadRequest(new ResponseDto
                    {
                        Success = false,
                        Message = "Ya existe un cliente con esa CI"
                    });
                }

                var cliente = new Cliente
                {
                    CI = CI,
                    Nombres = Nombres,
                    Direccion = Direccion,
                    Telefono = Telefono,
                    FechaRegistro = DateTime.Now
                };

                // Guardar fotos si existen
                if (fotoCasa1 != null)
                {
                    cliente.FotoCasa1 = await _archivoService.GuardarArchivoAsync(fotoCasa1, CI);
                }

                if (fotoCasa2 != null)
                {
                    cliente.FotoCasa2 = await _archivoService.GuardarArchivoAsync(fotoCasa2, CI);
                }

                if (fotoCasa3 != null)
                {
                    cliente.FotoCasa3 = await _archivoService.GuardarArchivoAsync(fotoCasa3, CI);
                }

                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();

                return Ok(new ResponseDto
                {
                    Success = true,
                    Message = "Cliente registrado exitosamente",
                    Data = cliente
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto
                {
                    Success = false,
                    Message = "Error al registrar cliente",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // GET: api/Clientes
        [HttpGet]
        public async Task<ActionResult<ResponseDto>> ObtenerClientes()
        {
            try
            {
                var clientes = await _context.Clientes
                    .Include(c => c.Archivos)
                    .ToListAsync();

                return Ok(new ResponseDto
                {
                    Success = true,
                    Message = "Clientes obtenidos exitosamente",
                    Data = clientes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto
                {
                    Success = false,
                    Message = "Error al obtener clientes",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // GET: api/Clientes/{ci}
        [HttpGet("{ci}")]
        public async Task<ActionResult<ResponseDto>> ObtenerClientePorCI(string ci)
        {
            try
            {
                var cliente = await _context.Clientes
                    .Include(c => c.Archivos)
                    .FirstOrDefaultAsync(c => c.CI == ci);

                if (cliente == null)
                {
                    return NotFound(new ResponseDto
                    {
                        Success = false,
                        Message = "Cliente no encontrado"
                    });
                }

                return Ok(new ResponseDto
                {
                    Success = true,
                    Message = "Cliente encontrado",
                    Data = cliente
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto
                {
                    Success = false,
                    Message = "Error al obtener cliente",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // PUT: api/Clientes/{ci}
        [HttpPut("{ci}")]
        public async Task<ActionResult<ResponseDto>> ActualizarCliente(
            string ci,
            [FromBody] ClienteDto clienteDto)
        {
            try
            {
                if (ci != clienteDto.CI)
                {
                    return BadRequest(new ResponseDto
                    {
                        Success = false,
                        Message = "La CI del parámetro no coincide con la del cuerpo de la solicitud"
                    });
                }

                var cliente = await _context.Clientes.FindAsync(ci);

                if (cliente == null)
                {
                    return NotFound(new ResponseDto
                    {
                        Success = false,
                        Message = "Cliente no encontrado"
                    });
                }

                cliente.Nombres = clienteDto.Nombres;
                cliente.Direccion = clienteDto.Direccion;
                cliente.Telefono = clienteDto.Telefono;

                _context.Entry(cliente).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new ResponseDto
                {
                    Success = true,
                    Message = "Cliente actualizado exitosamente",
                    Data = cliente
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ClienteExiste(ci))
                {
                    return NotFound(new ResponseDto
                    {
                        Success = false,
                        Message = "Cliente no encontrado"
                    });
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto
                {
                    Success = false,
                    Message = "Error al actualizar cliente",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // DELETE: api/Clientes/{ci}
        [HttpDelete("{ci}")]
        public async Task<ActionResult<ResponseDto>> EliminarCliente(string ci)
        {
            try
            {
                var cliente = await _context.Clientes
                    .Include(c => c.Archivos)
                    .FirstOrDefaultAsync(c => c.CI == ci);

                if (cliente == null)
                {
                    return NotFound(new ResponseDto
                    {
                        Success = false,
                        Message = "Cliente no encontrado"
                    });
                }

                // Eliminar archivos físicos del servidor
                if (!string.IsNullOrEmpty(cliente.FotoCasa1) && System.IO.File.Exists(cliente.FotoCasa1))
                {
                    System.IO.File.Delete(cliente.FotoCasa1);
                }

                if (!string.IsNullOrEmpty(cliente.FotoCasa2) && System.IO.File.Exists(cliente.FotoCasa2))
                {
                    System.IO.File.Delete(cliente.FotoCasa2);
                }

                if (!string.IsNullOrEmpty(cliente.FotoCasa3) && System.IO.File.Exists(cliente.FotoCasa3))
                {
                    System.IO.File.Delete(cliente.FotoCasa3);
                }

                // Eliminar archivos adicionales
                foreach (var archivo in cliente.Archivos)
                {
                    if (System.IO.File.Exists(archivo.UrlArchivo))
                    {
                        System.IO.File.Delete(archivo.UrlArchivo);
                    }
                }

                // Eliminar carpeta del cliente si está vacía
                var carpetaCliente = Path.Combine("Uploads", ci);
                if (Directory.Exists(carpetaCliente))
                {
                    try
                    {
                        Directory.Delete(carpetaCliente, true);
                    }
                    catch
                    {
                        // Si no se puede eliminar la carpeta, continuar
                    }
                }

                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();

                return Ok(new ResponseDto
                {
                    Success = true,
                    Message = "Cliente eliminado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto
                {
                    Success = false,
                    Message = "Error al eliminar cliente",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // Método auxiliar privado
        private async Task<bool> ClienteExiste(string ci)
        {
            return await _context.Clientes.AnyAsync(e => e.CI == ci);
        }
    }
}