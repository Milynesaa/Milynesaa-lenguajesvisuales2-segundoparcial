using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClientesAPI.Data;
using ClientesAPI.DTOs;

namespace ClientesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Logs
        [HttpGet]
        public async Task<ActionResult<ResponseDto>> ObtenerLogs(
            [FromQuery] string? tipoLog,
            [FromQuery] int pagina = 1,
            [FromQuery] int registrosPorPagina = 50)
        {
            var query = _context.LogsApi.AsQueryable();

            if (!string.IsNullOrEmpty(tipoLog))
            {
                query = query.Where(l => l.TipoLog == tipoLog);
            }

            var totalRegistros = await query.CountAsync();
            var logs = await query
                .OrderByDescending(l => l.FechaHora)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

            return Ok(new ResponseDto
            {
                Success = true,
                Message = "Logs obtenidos exitosamente",
                Data = new
                {
                    TotalRegistros = totalRegistros,
                    Pagina = pagina,
                    RegistrosPorPagina = registrosPorPagina,
                    TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)registrosPorPagina),
                    Logs = logs
                }
            });
        }

        // GET: api/Logs/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDto>> ObtenerLogPorId(int id)
        {
            var log = await _context.LogsApi.FindAsync(id);

            if (log == null)
            {
                return NotFound(new ResponseDto
                {
                    Success = false,
                    Message = "Log no encontrado"
                });
            }

            return Ok(new ResponseDto
            {
                Success = true,
                Message = "Log encontrado",
                Data = log
            });
        }

        // GET: api/Logs/errores
        [HttpGet("errores")]
        public async Task<ActionResult<ResponseDto>> ObtenerErrores([FromQuery] int cantidad = 100)
        {
            var errores = await _context.LogsApi
                .Where(l => l.TipoLog == "ERROR")
                .OrderByDescending(l => l.FechaHora)
                .Take(cantidad)
                .ToListAsync();

            return Ok(new ResponseDto
            {
                Success = true,
                Message = "Errores obtenidos exitosamente",
                Data = errores
            });
        }
    }
}