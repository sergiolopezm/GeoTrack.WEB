using GeoTrack.WEB.Models.Auth;
using GeoTrack.WEB.Models;

namespace GeoTrack.WEB.Services.Interface
{
    /// <summary>
    /// Interfaz para el servicio de autenticación
    /// </summary>
    public interface IAuthService
    {
        Task<RespuestaDto> LoginAsync(UsuarioLoginDto loginDto);
        Task<RespuestaDto> RegistrarUsuarioAsync(UsuarioRegistroDto registroDto);
        Task<RespuestaDto> ObtenerPerfilAsync();
        Task<bool> LogoutAsync();
        Task<UsuarioPerfilDto?> ObtenerUsuarioActual();
        bool EstaAutenticado();
    }
}
