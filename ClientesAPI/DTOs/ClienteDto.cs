using System.ComponentModel.DataAnnotations;

namespace ClientesAPI.DTOs
{
    public class ClienteDto
    {
        [Required(ErrorMessage = "La CI es obligatoria")]
        [MaxLength(20)]
        public string CI { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son obligatorios")]
        [MaxLength(200)]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [MaxLength(300)]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;
    }
}