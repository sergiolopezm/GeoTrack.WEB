using GeoTrack.WEB.Models.Ubicacion;
using GeoTrack.WEB.Models;
using GeoTrack.WEB.Services.Interface;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using GeoTrack.WEB.Helpers;

namespace GeoTrack.WEB.Services.Api
{

    /// <summary>
    /// Implementación del servicio de departamentos para comunicarse con la API
    /// </summary>
    public class ApiDepartamentoService : IDepartamentoService
    {
        private readonly HttpClient _httpClient;
        private readonly LocalStorageService _localStorage;
        private readonly ILogger<ApiDepartamentoService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        private const string BaseUrl = "departamento";

        public ApiDepartamentoService(
            HttpClient httpClient,
            LocalStorageService localStorage,
            ILogger<ApiDepartamentoService> logger)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        /// <summary>
        /// Configura los headers de autenticación para las peticiones HTTP
        /// </summary>
        private async Task ConfigurarHeadersAsync()
        {
            _httpClient.DefaultRequestHeaders.Clear();

            var token = await _localStorage.GetItemAsync<string>("authToken");
            var usuarioId = await _localStorage.GetItemAsync<string>("usuarioId");

            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(usuarioId))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                _httpClient.DefaultRequestHeaders.Add("UsuarioId", usuarioId);
            }
        }

        /// <summary>
        /// Obtiene todos los departamentos
        /// </summary>
        public async Task<List<DepartamentoDto>> ObtenerTodosAsync()
        {
            try
            {
                await ConfigurarHeadersAsync();

                var response = await _httpClient.GetAsync($"{BaseUrl}");

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<RespuestaDto>(_jsonOptions);

                    if (resultado != null && resultado.Exito && resultado.Resultado != null)
                    {
                        var departamentos = JsonSerializer.Deserialize<List<DepartamentoDto>>(
                            resultado.Resultado.ToString() ?? "[]",
                            _jsonOptions);

                        return departamentos ?? new List<DepartamentoDto>();
                    }
                }

                _logger.LogWarning("Error al obtener departamentos: {StatusCode}", response.StatusCode);
                return new List<DepartamentoDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener listado de departamentos");
                return new List<DepartamentoDto>();
            }
        }

        /// <summary>
        /// Obtiene los departamentos por país
        /// </summary>
        public async Task<List<DepartamentoDto>> ObtenerPorPaisIdAsync(int paisId)
        {
            try
            {
                await ConfigurarHeadersAsync();

                var response = await _httpClient.GetAsync($"{BaseUrl}/por-pais/{paisId}");

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<RespuestaDto>(_jsonOptions);

                    if (resultado != null && resultado.Exito && resultado.Resultado != null)
                    {
                        var departamentos = JsonSerializer.Deserialize<List<DepartamentoDto>>(
                            resultado.Resultado.ToString() ?? "[]",
                            _jsonOptions);

                        return departamentos ?? new List<DepartamentoDto>();
                    }
                }

                _logger.LogWarning("Error al obtener departamentos por país {PaisId}: {StatusCode}", paisId, response.StatusCode);
                return new List<DepartamentoDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener departamentos por país {PaisId}", paisId);
                return new List<DepartamentoDto>();
            }
        }

        /// <summary>
        /// Obtiene un departamento por su ID
        /// </summary>
        public async Task<DepartamentoDto?> ObtenerPorIdAsync(int id)
        {
            try
            {
                await ConfigurarHeadersAsync();

                var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<RespuestaDto>(_jsonOptions);

                    if (resultado != null && resultado.Exito && resultado.Resultado != null)
                    {
                        var departamento = JsonSerializer.Deserialize<DepartamentoDto>(
                            resultado.Resultado.ToString() ?? "",
                            _jsonOptions);

                        return departamento;
                    }
                }

                _logger.LogWarning("Error al obtener departamento {Id}: {StatusCode}", id, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener departamento {Id}", id);
                return null;
            }
        }

        /// <summary>
        /// Obtiene un listado paginado de departamentos
        /// </summary>
        public async Task<PaginacionDto<DepartamentoDto>> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, int? paisId = null, string? busqueda = null)
        {
            try
            {
                await ConfigurarHeadersAsync();

                var url = $"{BaseUrl}/paginado?pagina={pagina}&elementosPorPagina={elementosPorPagina}";
                if (paisId.HasValue)
                {
                    url += $"&paisId={paisId.Value}";
                }
                if (!string.IsNullOrEmpty(busqueda))
                {
                    url += $"&busqueda={Uri.EscapeDataString(busqueda)}";
                }

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<RespuestaDto>(_jsonOptions);

                    if (resultado != null && resultado.Exito && resultado.Resultado != null)
                    {
                        var paginacion = JsonSerializer.Deserialize<PaginacionDto<DepartamentoDto>>(
                            resultado.Resultado.ToString() ?? "",
                            _jsonOptions);

                        return paginacion ?? new PaginacionDto<DepartamentoDto>
                        {
                            Pagina = pagina,
                            ElementosPorPagina = elementosPorPagina,
                            TotalPaginas = 0,
                            TotalRegistros = 0,
                            Lista = new List<DepartamentoDto>()
                        };
                    }
                }

                _logger.LogWarning("Error al obtener departamentos paginados: {StatusCode}", response.StatusCode);
                return new PaginacionDto<DepartamentoDto>
                {
                    Pagina = pagina,
                    ElementosPorPagina = elementosPorPagina,
                    TotalPaginas = 0,
                    TotalRegistros = 0,
                    Lista = new List<DepartamentoDto>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener departamentos paginados");
                return new PaginacionDto<DepartamentoDto>
                {
                    Pagina = pagina,
                    ElementosPorPagina = elementosPorPagina,
                    TotalPaginas = 0,
                    TotalRegistros = 0,
                    Lista = new List<DepartamentoDto>()
                };
            }
        }

        /// <summary>
        /// Crea un nuevo departamento
        /// </summary>
        public async Task<RespuestaDto> CrearAsync(DepartamentoDto departamentoDto)
        {
            try
            {
                await ConfigurarHeadersAsync();

                var content = new StringContent(JsonSerializer.Serialize(departamentoDto), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(BaseUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<RespuestaDto>(_jsonOptions);
                    return resultado ?? RespuestaDto.ErrorPorDefecto();
                }
                else
                {
                    var errorContent = await response.Content.ReadFromJsonAsync<RespuestaDto>(_jsonOptions);
                    return errorContent ?? new RespuestaDto
                    {
                        Exito = false,
                        Mensaje = "Error al crear departamento",
                        Detalle = $"Código de error HTTP: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear departamento {Nombre}", departamentoDto.Nombre);
                return RespuestaDto.Desde(ex);
            }
        }

        /// <summary>
        /// Actualiza un departamento existente
        /// </summary>
        public async Task<RespuestaDto> ActualizarAsync(int id, DepartamentoDto departamentoDto)
        {
            try
            {
                await ConfigurarHeadersAsync();

                var content = new StringContent(JsonSerializer.Serialize(departamentoDto), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{BaseUrl}/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<RespuestaDto>(_jsonOptions);
                    return resultado ?? RespuestaDto.ErrorPorDefecto();
                }
                else
                {
                    var errorContent = await response.Content.ReadFromJsonAsync<RespuestaDto>(_jsonOptions);
                    return errorContent ?? new RespuestaDto
                    {
                        Exito = false,
                        Mensaje = "Error al actualizar departamento",
                        Detalle = $"Código de error HTTP: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar departamento {Id}", id);
                return RespuestaDto.Desde(ex);
            }
        }

        /// <summary>
        /// Elimina un departamento
        /// </summary>
        public async Task<RespuestaDto> EliminarAsync(int id)
        {
            try
            {
                await ConfigurarHeadersAsync();

                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<RespuestaDto>(_jsonOptions);
                    return resultado ?? RespuestaDto.ErrorPorDefecto();
                }
                else
                {
                    var errorContent = await response.Content.ReadFromJsonAsync<RespuestaDto>(_jsonOptions);
                    return errorContent ?? new RespuestaDto
                    {
                        Exito = false,
                        Mensaje = "Error al eliminar departamento",
                        Detalle = $"Código de error HTTP: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar departamento {Id}", id);
                return RespuestaDto.Desde(ex);
            }
        }
    }
}
