namespace GeoTrack.WEB.Models.Auth
{
    /// <summary>
    /// DTO para token de autenticación
    /// </summary>
    public class TokenDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiracion { get; set; }
        public UsuarioPerfilDto Usuario { get; set; } = null!;
    }
}
