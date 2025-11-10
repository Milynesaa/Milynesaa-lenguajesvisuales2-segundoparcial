using System.ComponentModel.DataAnnotations;

namespace ClientesAPI.Models
{
    public class Cliente
    {
        [Key]
        [MaxLength(20)]
        public string CI { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Nombres { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Direccion { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        // Fotos como rutas de archivos
        [MaxLength(500)]
        public string? FotoCasa1 { get; set; }

        [MaxLength(500)]
        public string? FotoCasa2 { get; set; }

        [MaxLength(500)]
        public string? FotoCasa3 { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Relación con archivos
        public virtual ICollection<ArchivoCliente> Archivos { get; set; } = new List<ArchivoCliente>();
    }
}