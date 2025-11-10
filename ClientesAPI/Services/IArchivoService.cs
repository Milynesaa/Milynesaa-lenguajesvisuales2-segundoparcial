namespace ClientesAPI.Services
{
    public interface IArchivoService
    {
        Task<string> GuardarArchivoAsync(IFormFile archivo, string ciCliente);
        Task<List<string>> DescomprimirYGuardarAsync(IFormFile archivoZip, string ciCliente);
    }
}