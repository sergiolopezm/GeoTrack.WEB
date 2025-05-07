using System.ComponentModel.DataAnnotations;

namespace GeoTrack.WEB.Models.Auth
{
    /// <summary>
    /// DTO para el login de usuario
    /// </summary>
    public class UsuarioLoginDto
    {
        [Required(ErrorMessage = "El usuario es requerido")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Contraseña { get; set; } = string.Empty;

        public string? Ip { get; set; }
    }
}
