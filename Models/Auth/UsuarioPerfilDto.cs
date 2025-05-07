namespace GeoTrack.WEB.Models.Auth
{
    /// <summary>
    /// DTO para mostrar información de perfil de usuario
    /// </summary>
    public class UsuarioPerfilDto
    {
        public Guid Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int RolId { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaUltimoAcceso { get; set; }

        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}
