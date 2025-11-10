using System.ComponentModel.DataAnnotations;

namespace ClientesAPI.Models
{
    public class LogApi
    {
        [Key]
        public int IdLog { get; set; }

        public DateTime FechaHora { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string TipoLog { get; set; } = string.Empty; // INFO, ERROR, WARNING

        public string? RequestBody { get; set; }

        public string? ResponseBody { get; set; }

        [MaxLength(500)]
        public string? UrlEndpoint { get; set; }

        [MaxLength(10)]
        public string? MetodoHttp { get; set; }

        [MaxLength(50)]
        public string? DireccionIp { get; set; }

        public string? Detalle { get; set; }

        public int? CodigoEstado { get; set; }
    }
}