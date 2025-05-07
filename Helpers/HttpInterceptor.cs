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
                await AddHeadersAsync(request);

                Console.WriteLine($"Enviando solicitud a: {request.RequestUri}");
                Console.WriteLine($"Método: {request.Method}");
                Console.WriteLine($"Headers: {string.Join(", ", request.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");

                var response = await base.SendAsync(request, cancellationToken);

                Console.WriteLine($"Respuesta recibida: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {content}");
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _nav.NavigateTo("/auth/login", forceLoad: true);
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en HttpInterceptor: {ex.Message}");
                throw; // Re-lanzar para mantener la cadena de error
            }
        }

        // ------------------------------------------------------
        private async Task AddHeadersAsync(HttpRequestMessage request)
        {
            try
            {
                request.Headers.Add("Sitio", "GeoTrack");
                request.Headers.Add("Clave", "GeoTrack2025");

                var token = await _storage.GetItemAsync<string>("authToken");
                var usuarioId = await _storage.GetItemAsync<string>("usuarioId");

                if (!string.IsNullOrWhiteSpace(token))
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                if (!string.IsNullOrWhiteSpace(usuarioId))
                    request.Headers.Add("UsuarioId", usuarioId);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error agregando encabezados HTTP");
            }
        }
    }
}
