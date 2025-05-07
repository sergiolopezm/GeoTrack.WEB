using GeoTrack.WEB.Models.Auth;
using GeoTrack.WEB.Models;
using GeoTrack.WEB.Services.Interface;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using GeoTrack.WEB.Helpers;

namespace GeoTrack.WEB.Services.Api
{
    /// <summary>
    /// Implementación del servicio de autenticación para comunicarse con la API
    /// </summary>
    public class ApiAuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly LocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILogger<ApiAuthService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiAuthService(
            HttpClient httpClient,
            LocalStorageService localStorage,
            AuthenticationStateProvider authStateProvider,
            ILogger<ApiAuthService> logger)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        /// <summary>
        /// Realiza el login en la API
        /// </summary>
        public async Task<RespuestaDto> LoginAsync(UsuarioLoginDto loginDto)
        {
            try
            {
                // Agregar headers para acceso a la API
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Sitio", "GeoTrack");
                _httpClient.DefaultRequestHeaders.Add("Clave", "GeoTrack2025");

                var content = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<RespuestaDto>(_jsonOptions);

                    if (resultado != null && resultado.Exito && resultado.Resultado != null)
                    {
                        var authData = JsonSerializer.Deserialize<AuthResultData>(
                            resultado.Resultado.ToString() ?? "",
                            _jsonOptions);

                        if (authData != null)
                        {
                            // Guardar token y datos de usuario en localStorage
                            await _localStorage.SetItemAsync("authToken", authData.Token);
                            await _localStorage.SetItemAsync("usuarioId", authData.Usuario.Id);
                            await _localStorage.SetItemAsync("usuario", JsonSerializer.Serialize(authData.Usuario));

                            // Notificar al estado de autenticación
                            ((JwtAuthenticationStateProvider)_authStateProvider).NotificarLogin(authData.Token);

                            return resultado;
                        }
                    }

                    return resultado ?? RespuestaDto.ErrorPorDefecto();
                }
                else
                {
                    var errorContent = await response.Content.ReadFromJsonAsync<RespuestaDto>(_jsonOptions);
                    return errorContent ?? new RespuestaDto
                    {
                        Exito = false,
                        Mensaje = "Error de autenticación",
                        Detalle = $"Código de error HTTP: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login");
                return RespuestaDto.Desde(ex);
            }
        }

        /// <summary>
        /// Registra un nuevo usuario en la API
        /// </summary>
        public async Task<RespuestaDto> RegistrarUsuarioAsync(UsuarioRegistroDto registroDto)
        {
            try
            {
                // Agregar headers para acceso a la API y autenticación JWT
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Sitio", "GeoTrack");
                _httpClient.DefaultRequestHeaders.Add("Clave", "GeoTrack2025");

                var token = await _localStorage.GetItemAsync<string>("authToken");
                var usuarioId = await _localStorage.GetItemAsync<string>("usuarioId");

                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(usuarioId))
                {
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    _httpClient.DefaultRequestHeaders.Add("UsuarioId", usuarioId);
                }

                var content = new StringContent(JsonSerializer.Serialize(registroDto), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("auth/registro", content);

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
                        Mensaje = "Error en el registro",
                        Detalle = $"Código de error HTTP: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en registro de usuario");
                return RespuestaDto.Desde(ex);
            }
        }

        /// <summary>
        /// Obtiene el perfil del usuario actual
        /// </summary>
        public async Task<RespuestaDto> ObtenerPerfilAsync()
        {
            try
            {
                // Agregar headers para autenticación JWT
                _httpClient.DefaultRequestHeaders.Clear();

                var token = await _localStorage.GetItemAsync<string>("authToken");
                var usuarioId = await _localStorage.GetItemAsync<string>("usuarioId");

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(usuarioId))
                {
                    return new RespuestaDto
                    {
                        Exito = false,
                        Mensaje = "No autenticado",
                        Detalle = "Debe iniciar sesión para acceder a esta funcionalidad"
                    };
                }

                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                _httpClient.DefaultRequestHeaders.Add("UsuarioId", usuarioId);

                var response = await _httpClient.GetAsync("auth/perfil");

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<RespuestaDto>(_jsonOptions);

                    if (resultado != null && resultado.Exito && resultado.Resultado != null)
                    {
                        var usuario = JsonSerializer.Deserialize<UsuarioPerfilDto>(
                            resultado.Resultado.ToString() ?? "",
                            _jsonOptions);

                        if (usuario != null)
                        {
                            // Actualizar usuario en localStorage
                            await _localStorage.SetItemAsync("usuario", JsonSerializer.Serialize(usuario));
                        }
                    }

                    return resultado ?? RespuestaDto.ErrorPorDefecto();
                }
                else
                {
                    var errorContent = await response.Content.ReadFromJsonAsync<RespuestaDto>(_jsonOptions);
                    return errorContent ?? new RespuestaDto
                    {
                        Exito = false,
                        Mensaje = "Error al obtener perfil",
                        Detalle = $"Código de error HTTP: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener perfil de usuario");
                return RespuestaDto.Desde(ex);
            }
        }

        /// <summary>
        /// Cierra la sesión del usuario actual
        /// </summary>
        public async Task<bool> LogoutAsync()
        {
            try
            {
                // Agregar headers para autenticación JWT
                _httpClient.DefaultRequestHeaders.Clear();

                var token = await _localStorage.GetItemAsync<string>("authToken");
                var usuarioId = await _localStorage.GetItemAsync<string>("usuarioId");

                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(usuarioId))
                {
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    _httpClient.DefaultRequestHeaders.Add("UsuarioId", usuarioId);

                    // Llamar al endpoint de logout
                    var response = await _httpClient.PostAsync("auth/logout", null);

                    // Independientemente del resultado, limpiar el almacenamiento local
                    await _localStorage.RemoveItemAsync("authToken");
                    await _localStorage.RemoveItemAsync("usuarioId");
                    await _localStorage.RemoveItemAsync("usuario");

                    // Notificar al estado de autenticación
                    ((JwtAuthenticationStateProvider)_authStateProvider).NotificarLogout();

                    return response.IsSuccessStatusCode;
                }

                // Si no hay token, simplemente limpiamos el storage
                await _localStorage.RemoveItemAsync("authToken");
                await _localStorage.RemoveItemAsync("usuarioId");
                await _localStorage.RemoveItemAsync("usuario");

                // Notificar al estado de autenticación
                ((JwtAuthenticationStateProvider)_authStateProvider).NotificarLogout();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en logout");
                // Aún así, intentamos limpiar el storage
                await _localStorage.RemoveItemAsync("authToken");
                await _localStorage.RemoveItemAsync("usuarioId");
                await _localStorage.RemoveItemAsync("usuario");

                // Notificar al estado de autenticación
                ((JwtAuthenticationStateProvider)_authStateProvider).NotificarLogout();

                return false;
            }
        }

        /// <summary>
        /// Obtiene los datos del usuario actual desde localStorage
        /// </summary>
        public async Task<UsuarioPerfilDto?> ObtenerUsuarioActual()
        {
            try
            {
                var usuarioJson = await _localStorage.GetItemAsync<string>("usuario");

                if (string.IsNullOrEmpty(usuarioJson))
                    return null;

                return JsonSerializer.Deserialize<UsuarioPerfilDto>(usuarioJson, _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Verifica si el usuario está autenticado
        /// </summary>
        public bool EstaAutenticado()
        {
            var token = _localStorage.GetItemAsync<string>("authToken").Result;
            return !string.IsNullOrEmpty(token);
        }

        /// <summary>
        /// Clase auxiliar para deserializar la respuesta de autenticación
        /// </summary>
        private class AuthResultData
        {
            public string Token { get; set; } = string.Empty;
            public UsuarioPerfilDto Usuario { get; set; } = null!;
        }
    }
}
