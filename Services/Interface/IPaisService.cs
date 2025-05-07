using GeoTrack.WEB.Models.Ubicacion;
using GeoTrack.WEB.Models;

namespace GeoTrack.WEB.Services.Interface
{
    /// <summary>
    /// Interfaz para el servicio de países
    /// </summary>
    public interface IPaisService
    {
        Task<List<PaisDto>> ObtenerTodosAsync();
        Task<PaisDto?> ObtenerPorIdAsync(int id);
        Task<PaginacionDto<PaisDto>> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, string? busqueda = null);
        Task<RespuestaDto> CrearAsync(PaisDto paisDto);
        Task<RespuestaDto> ActualizarAsync(int id, PaisDto paisDto);
        Task<RespuestaDto> EliminarAsync(int id);
    }
}
