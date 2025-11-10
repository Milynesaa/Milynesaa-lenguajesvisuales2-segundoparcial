using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientesAPI.Models
{
    public class ArchivoCliente
    {
        [Key]
        public int IdArchivo { get; set; }

        [Required]
        [MaxLength(20)]
        public string CICliente { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string NombreArchivo { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string UrlArchivo { get; set; } = string.Empty;

        public DateTime FechaSubida { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string? TipoArchivo { get; set; }

        public long TamañoBytes { get; set; }

        // Relación con Cliente
        [ForeignKey("CICliente")]
        public virtual Cliente? Cliente { get; set; }
    }
}