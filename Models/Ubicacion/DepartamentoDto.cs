using System.ComponentModel.DataAnnotations;

namespace GeoTrack.WEB.Models.Ubicacion
{
    /// <summary>
    /// DTO para departamentos
    /// </summary>
    public class DepartamentoDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del departamento es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El país es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un país válido")]
        public int PaisId { get; set; }

        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public Guid? CreadoPorId { get; set; }
        public Guid? ModificadoPorId { get; set; }

        // Propiedades de navegación para UI
        public string? Pais { get; set; }
        public string? CreadoPor { get; set; }
        public string? ModificadoPor { get; set; }

        // Contador de ciudades asociadas
        public int CiudadesCount { get; set; }
    }
}
