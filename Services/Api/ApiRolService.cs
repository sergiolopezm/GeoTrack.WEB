using GeoTrack.WEB.Helpers;
using GeoTrack.WEB.Models.Auth;
using GeoTrack.WEB.Models;
using GeoTrack.WEB.Services.Interface;
using System.Net.Http.Json;
using System.Text.Json;

namespace GeoTrack.WEB.Services.Api
{
    /// <summary>
    /// Implementación del servicio de roles para comunicarse con la API
    /// </summary>
    public class ApiRolService : IRolService
    {
        private readonly HttpClient _httpClient;
        private readonly LocalStorageService _localStorage;
        private readonly ILogger<ApiRolService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        private const string BaseUrl = "rol";

        public ApiRolService(
            HttpClient httpClient,
            LocalStorageService localStorage,
            ILogger<ApiRolService> logger)
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
        /// Obtiene todos los roles
        /// </summary>
        public async Task<List<RolDto>> ObtenerTodosAsync()
        {
            try
            {
                await ConfigurarHeadersAsync();

                var response = await _httpClient.GetAsync(BaseUrl);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<RespuestaDto>(_jsonOptions);

                    if (resultado != null && resultado.Exito && resultado.Resultado != null)
                    {
                        var roles = JsonSerializer.Deserialize<List<RolDto>>(
                            resultado.Resultado.ToString() ?? "[]",
                            _jsonOptions);

                        return roles ?? new List<RolDto>();
                    }
                }

                _logger.LogWarning("Error al obtener roles: {StatusCode}", response.StatusCode);
                return new List<RolDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener listado de roles");
                return new List<RolDto>();
            }
        }

        /// <summary>
        /// Obtiene un rol por su ID
        /// </summary>
        public async Task<RolDto?> ObtenerPorIdAsync(int id)
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
                        var rol = JsonSerializer.Deserialize<RolDto>(
                            resultado.Resultado.ToString() ?? "",
                            _jsonOptions);

                        return rol;
                    }
                }

                _logger.LogWarning("Error al obtener rol {Id}: {StatusCode}", id, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener rol {Id}", id);
                return null;
            }
        }

        /// <summary>
        /// Obtiene un rol por su nombre
        /// </summary>
        public async Task<RolDto?> ObtenerPorNombreAsync(string nombre)
        {
            try
            {
                await ConfigurarHeadersAsync();

                var response = await _httpClient.GetAsync($"{BaseUrl}/nombre/{nombre}");

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<RespuestaDto>(_jsonOptions);

                    if (resultado != null && resultado.Exito && resultado.Resultado != null)
                    {
                        var rol = JsonSerializer.Deserialize<RolDto>(
                            resultado.Resultado.ToString() ?? "",
                            _jsonOptions);

                        return rol;
                    }
                }

                _logger.LogWarning("Error al obtener rol por nombre {Nombre}: {StatusCode}", nombre, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener rol por nombre {Nombre}", nombre);
                return null;
            }
        }

        /// <summary>
        /// Verifica si existe un rol con el ID especificado
        /// </summary>
        public async Task<bool> ExisteAsync(int id)
        {
            try
            {
                await ConfigurarHeadersAsync();

                var response = await _httpClient.GetAsync($"{BaseUrl}/existe/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
                    return resultado;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de rol {Id}", id);
                return false;
            }
        }

        /// <summary>
        /// Verifica si existe un rol con el nombre especificado
        /// </summary>
        public async Task<bool> ExistePorNombreAsync(string nombre)
        {
            try
            {
                await ConfigurarHeadersAsync();

                var response = await _httpClient.GetAsync($"{BaseUrl}/existe-nombre/{nombre}");

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
                    return resultado;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de rol por nombre {Nombre}", nombre);
                return false;
            }
        }
    }
}
