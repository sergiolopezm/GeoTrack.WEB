using GeoTrack.WEB.Models.Auth;

namespace GeoTrack.WEB.Services.Interface
{
    /// <summary>
    /// Interfaz para el servicio de roles
    /// </summary>
    public interface IRolService
    {
        Task<List<RolDto>> ObtenerTodosAsync();
        Task<RolDto?> ObtenerPorIdAsync(int id);
        Task<RolDto?> ObtenerPorNombreAsync(string nombre);
        Task<bool> ExisteAsync(int id);
        Task<bool> ExistePorNombreAsync(string nombre);
    }
}
