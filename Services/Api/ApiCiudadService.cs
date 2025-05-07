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
    /// Implementación del servicio de ciudades para comunicarse con la API
    /// </summary>
    public class ApiCiudadService : ICiudadService
    {
        private readonly HttpClient _httpClient;
        private readonly LocalStorageService _localStorage;
        private readonly ILogger<ApiCiudadService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        private const string BaseUrl = "ciudad";

        public ApiCiudadService(
            HttpClient httpClient,
            LocalStorageService localStorage,
            ILogger<ApiCiudadService> logger)
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
        /// Obtiene todas las ciudades
        /// </summary>
        public async Task<List<CiudadDto>> ObtenerTodosAsync()
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
                        var ciudades = JsonSerializer.Deserialize<List<CiudadDto>>(
                            resultado.Resultado.ToString() ?? "[]",
                            _jsonOptions);

                        return ciudades ?? new List<CiudadDto>();
                    }
                }

                _logger.LogWarning("Error al obtener ciudades: {StatusCode}", response.StatusCode);
                return new List<CiudadDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener listado de ciudades");
                return new List<CiudadDto>();
            }
        }

        /// <summary>
        /// Obtiene las ciudades por departamento
        /// </summary>
        public async Task<List<CiudadDto>> ObtenerPorDepartamentoIdAsync(int departamentoId)
        {
            try
            {
                await ConfigurarHeadersAsync();

                var response = await _httpClient.GetAsync($"{BaseUrl}/por-departamento/{departamentoId}");

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<RespuestaDto>(_jsonOptions);

                    if (resultado != null && resultado.Exito && resultado.Resultado != null)
                    {
                        var ciudades = JsonSerializer.Deserialize<List<CiudadDto>>(
                            resultado.Resultado.ToString() ?? "[]",
                            _jsonOptions);

                        return ciudades ?? new List<CiudadDto>();
                    }
                }

                _logger.LogWarning("Error al obtener ciudades por departamento {DepartamentoId}: {StatusCode}", departamentoId, response.StatusCode);
                return new List<CiudadDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ciudades por departamento {DepartamentoId}", departamentoId);
                return new List<CiudadDto>();
            }
        }

        /// <summary>
        /// Obtiene una ciudad por su ID
        /// </summary>
        public async Task<CiudadDto?> ObtenerPorIdAsync(int id)
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
                        var ciudad = JsonSerializer.Deserialize<CiudadDto>(
                            resultado.Resultado.ToString() ?? "",
                            _jsonOptions);

                        return ciudad;
                    }
                }

                _logger.LogWarning("Error al obtener ciudad {Id}: {StatusCode}", id, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ciudad {Id}", id);
                return null;
            }
        }

        /// <summary>
        /// Obtiene un listado paginado de ciudades
        /// </summary>
        public async Task<PaginacionDto<CiudadDto>> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, int? departamentoId = null, string? busqueda = null)
        {
            try
            {
                await ConfigurarHeadersAsync();

                var url = $"{BaseUrl}/paginado?pagina={pagina}&elementosPorPagina={elementosPorPagina}";
                if (departamentoId.HasValue)
                {
                    url += $"&departamentoId={departamentoId.Value}";
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
                        var paginacion = JsonSerializer.Deserialize<PaginacionDto<CiudadDto>>(
                            resultado.Resultado.ToString() ?? "",
                            _jsonOptions);

                        return paginacion ?? new PaginacionDto<CiudadDto>
                        {
                            Pagina = pagina,
                            ElementosPorPagina = elementosPorPagina,
                            TotalPaginas = 0,
                            TotalRegistros = 0,
                            Lista = new List<CiudadDto>()
                        };
                    }
                }

                _logger.LogWarning("Error al obtener ciudades paginadas: {StatusCode}", response.StatusCode);
                return new PaginacionDto<CiudadDto>
                {
                    Pagina = pagina,
                    ElementosPorPagina = elementosPorPagina,
                    TotalPaginas = 0,
                    TotalRegistros = 0,
                    Lista = new List<CiudadDto>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ciudades paginadas");
                return new PaginacionDto<CiudadDto>
                {
                    Pagina = pagina,
                    ElementosPorPagina = elementosPorPagina,
                    TotalPaginas = 0,
                    TotalRegistros = 0,
                    Lista = new List<CiudadDto>()
                };
            }
        }

        /// <summary>
        /// Crea una nueva ciudad
        /// </summary>
        public async Task<RespuestaDto> CrearAsync(CiudadDto ciudadDto)
        {
            try
            {
                await ConfigurarHeadersAsync();

                var content = new StringContent(JsonSerializer.Serialize(ciudadDto), Encoding.UTF8, "application/json");
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
                        Mensaje = "Error al crear ciudad",
                        Detalle = $"Código de error HTTP: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear ciudad {Nombre}", ciudadDto.Nombre);
                return RespuestaDto.Desde(ex);
            }
        }

        /// <summary>
        /// Actualiza una ciudad existente
        /// </summary>
        public async Task<RespuestaDto> ActualizarAsync(int id, CiudadDto ciudadDto)
        {
            try
            {
                await ConfigurarHeadersAsync();

                var content = new StringContent(JsonSerializer.Serialize(ciudadDto), Encoding.UTF8, "application/json");
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
                        Mensaje = "Error al actualizar ciudad",
                        Detalle = $"Código de error HTTP: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar ciudad {Id}", id);
                return RespuestaDto.Desde(ex);
            }
        }

        /// <summary>
        /// Elimina una ciudad
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
                        Mensaje = "Error al eliminar ciudad",
                        Detalle = $"Código de error HTTP: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar ciudad {Id}", id);
                return RespuestaDto.Desde(ex);
            }
        }
    }
}
