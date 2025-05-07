using GeoTrack.WEB.Models.Ubicacion;
using GeoTrack.WEB.Models;

namespace GeoTrack.WEB.Services.Interface
{
    /// <summary>
    /// Interfaz para el servicio de ciudades
    /// </summary>
    public interface ICiudadService
    {
        Task<List<CiudadDto>> ObtenerTodosAsync();
        Task<List<CiudadDto>> ObtenerPorDepartamentoIdAsync(int departamentoId);
        Task<CiudadDto?> ObtenerPorIdAsync(int id);
        Task<PaginacionDto<CiudadDto>> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, int? departamentoId = null, string? busqueda = null);
        Task<RespuestaDto> CrearAsync(CiudadDto ciudadDto);
        Task<RespuestaDto> ActualizarAsync(int id, CiudadDto ciudadDto);
        Task<RespuestaDto> EliminarAsync(int id);
    }
}
