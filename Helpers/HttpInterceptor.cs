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
            await AddHeadersAsync(request);
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _nav.NavigateTo("/auth/login", forceLoad: true);
            }

            return response;
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
