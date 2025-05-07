using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace GeoTrack.WEB.Helpers
{
    /// <summary>
    /// Inserta encabezados comunes y redirige al login si la API responde 401.
    /// </summary>
    public class HttpInterceptor : DelegatingHandler
    {
        private readonly LocalStorageService _storage;
        private readonly NavigationManager _nav;
        private readonly ILogger<HttpInterceptor> _log;

        public HttpInterceptor(
            LocalStorageService storage,
            NavigationManager nav,
            ILogger<HttpInterceptor> log)
        {
            _storage = storage;
            _nav = nav;
            _log = log;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si es una solicitud de Login para manejarla de forma especial
                bool esRutaLogin = request.RequestUri?.PathAndQuery?.Contains("/auth/login") ?? false;

                // Solo agregar headers completos si no es la ruta de login
                if (!esRutaLogin)
                {
                    await AddHeadersAsync(request);
                }
                else
                {
                    // Para login solo agregar headers básicos
                    request.Headers.Add("Sitio", "GeoTrack");
                    request.Headers.Add("Clave", "GeoTrack2025");
                }

                _log.LogInformation($"Enviando solicitud a: {request.RequestUri}");
                _log.LogInformation($"Método: {request.Method}");

                // Agregar un timeout para evitar bloqueos indefinidos
                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(30)); // 30 segundos máximo

                var response = await base.SendAsync(request, cts.Token);

                _log.LogInformation($"Respuesta recibida: {response.StatusCode}");

                // Solo redireccionar a login si no estamos ya en esa página
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var currentUri = _nav.Uri;
                    // Solo redireccionar si no estamos ya en la página de login
                    // y asegurarnos de que hay un token en el almacenamiento
                    var token = await _storage.GetItemAsync<string>("authToken");
                    if (!currentUri.Contains("/auth/login") && !string.IsNullOrEmpty(token))
                    {
                        _log.LogWarning("Redirección a login debido a 401 Unauthorized");
                        _nav.NavigateTo("/auth/login", forceLoad: false);
                    }
                }

                return response;
            }
            catch (TaskCanceledException ex)
            {
                _log.LogError(ex, "La solicitud fue cancelada por timeout");
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                {
                    Content = new StringContent("La solicitud excedió el tiempo de espera")
                };
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error en HttpInterceptor: {Message}", ex.Message);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent($"Error: {ex.Message}")
                };
            }
        }

        private async Task AddHeadersAsync(HttpRequestMessage request)
        {
            try
            {
                // Verificar primero si el header ya existe para evitar duplicados
                if (!request.Headers.Contains("Sitio"))
                    request.Headers.Add("Sitio", "GeoTrack");

                if (!request.Headers.Contains("Clave"))
                    request.Headers.Add("Clave", "GeoTrack2025");

                var token = await _storage.GetItemAsync<string>("authToken");
                var usuarioId = await _storage.GetItemAsync<string>("usuarioId");

                if (!string.IsNullOrWhiteSpace(token) && !request.Headers.Contains("Authorization"))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                if (!string.IsNullOrWhiteSpace(usuarioId) && !request.Headers.Contains("UsuarioId"))
                    request.Headers.Add("UsuarioId", usuarioId);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error agregando encabezados HTTP");
            }
        }
    }
}