using GeoTrack.WEB.Models.Ubicacion;
using GeoTrack.WEB.Models;

namespace GeoTrack.WEB.Services.Interface
{
    /// <summary>
    /// Interfaz para el servicio de departamentos
    /// </summary>
    public interface IDepartamentoService
    {
        Task<List<DepartamentoDto>> ObtenerTodosAsync();
        Task<List<DepartamentoDto>> ObtenerPorPaisIdAsync(int paisId);
        Task<DepartamentoDto?> ObtenerPorIdAsync(int id);
        Task<PaginacionDto<DepartamentoDto>> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, int? paisId = null, string? busqueda = null);
        Task<RespuestaDto> CrearAsync(DepartamentoDto departamentoDto);
        Task<RespuestaDto> ActualizarAsync(int id, DepartamentoDto departamentoDto);
        Task<RespuestaDto> EliminarAsync(int id);
    }
}
