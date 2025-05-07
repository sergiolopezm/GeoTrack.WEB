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
    /// Implementación del servicio de países para comunicarse con la API
    /// </summary>
    public class ApiPaisService : IPaisService
    {
        private readonly HttpClient _httpClient;
        private readonly LocalStorageService _localStorage;
        private readonly ILogger<ApiPaisService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        private const string BaseUrl = "pais";

        public ApiPaisService(
            HttpClient httpClient,
            LocalStorageService localStorage,
            ILogger<ApiPaisService> logger)
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
        /// Obtiene todos los países
        /// </summary>
        public async Task<List<PaisDto>> ObtenerTodosAsync()
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
                        var paises = JsonSerializer.Deserialize<List<PaisDto>>(
                            resultado.Resultado.ToString() ?? "[]",
                            _jsonOptions);

                        return paises ?? new List<PaisDto>();
                    }
                }

                _logger.LogWarning("Error al obtener países: {StatusCode}", response.StatusCode);
                return new List<PaisDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener listado de países");
                return new List<PaisDto>();
            }
        }

        /// <summary>
        /// Obtiene un país por su ID
        /// </summary>
        public async Task<PaisDto?> ObtenerPorIdAsync(int id)
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
                        var pais = JsonSerializer.Deserialize<PaisDto>(
                            resultado.Resultado.ToString() ?? "",
                            _jsonOptions);

                        return pais;
                    }
                }

                _logger.LogWarning("Error al obtener país {Id}: {StatusCode}", id, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener país {Id}", id);
                return null;
            }
        }

        /// <summary>
        /// Obtiene un listado paginado de países
        /// </summary>
        public async Task<PaginacionDto<PaisDto>> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, string? busqueda = null)
        {
            try
            {
                await ConfigurarHeadersAsync();

                var url = $"{BaseUrl}/paginado?pagina={pagina}&elementosPorPagina={elementosPorPagina}";
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
                        var paginacion = JsonSerializer.Deserialize<PaginacionDto<PaisDto>>(
                            resultado.Resultado.ToString() ?? "",
                            _jsonOptions);

                        return paginacion ?? new PaginacionDto<PaisDto>
                        {
                            Pagina = pagina,
                            ElementosPorPagina = elementosPorPagina,
                            TotalPaginas = 0,
                            TotalRegistros = 0,
                            Lista = new List<PaisDto>()
                        };
                    }
                }

                _logger.LogWarning("Error al obtener países paginados: {StatusCode}", response.StatusCode);
                return new PaginacionDto<PaisDto>
                {
                    Pagina = pagina,
                    ElementosPorPagina = elementosPorPagina,
                    TotalPaginas = 0,
                    TotalRegistros = 0,
                    Lista = new List<PaisDto>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener países paginados");
                return new PaginacionDto<PaisDto>
                {
                    Pagina = pagina,
                    ElementosPorPagina = elementosPorPagina,
                    TotalPaginas = 0,
                    TotalRegistros = 0,
                    Lista = new List<PaisDto>()
                };
            }
        }

        /// <summary>
        /// Crea un nuevo país
        /// </summary>
        public async Task<RespuestaDto> CrearAsync(PaisDto paisDto)
        {
            try
            {
                await ConfigurarHeadersAsync();

                var content = new StringContent(JsonSerializer.Serialize(paisDto), Encoding.UTF8, "application/json");
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
                        Mensaje = "Error al crear país",
                        Detalle = $"Código de error HTTP: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear país {Nombre}", paisDto.Nombre);
                return RespuestaDto.Desde(ex);
            }
        }

        /// <summary>
        /// Actualiza un país existente
        /// </summary>
        public async Task<RespuestaDto> ActualizarAsync(int id, PaisDto paisDto)
        {
            try
            {
                await ConfigurarHeadersAsync();

                var content = new StringContent(JsonSerializer.Serialize(paisDto), Encoding.UTF8, "application/json");
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
                        Mensaje = "Error al actualizar país",
                        Detalle = $"Código de error HTTP: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar país {Id}", id);
                return RespuestaDto.Desde(ex);
            }
        }

        /// <summary>
        /// Elimina un país
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
                        Mensaje = "Error al eliminar país",
                        Detalle = $"Código de error HTTP: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar país {Id}", id);
                return RespuestaDto.Desde(ex);
            }
        }
    }
}
