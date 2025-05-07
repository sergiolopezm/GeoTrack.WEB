using System.Net.Http.Headers;

namespace GeoTrack.WEB.Helpers
{
    /// <summary>
    /// Interceptor para añadir headers de autenticación a las solicitudes HTTP
    /// </summary>
    public class HttpInterceptor : DelegatingHandler
    {
        private readonly LocalStorageService _localStorage;
        private readonly ILogger<HttpInterceptor> _logger;

        public HttpInterceptor(LocalStorageService localStorage, ILogger<HttpInterceptor> logger)
        {
            _localStorage = localStorage;
            _logger = logger;
            InnerHandler = new HttpClientHandler();
        }

        /// <summary>
        /// Intercepta las solicitudes HTTP y añade los headers de autenticación
        /// </summary>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await AgregarHeadersAutenticacionAsync(request);

            try
            {
                // Llamar al handler interno para continuar con la solicitud
                return await base.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en la solicitud HTTP: {Url}", request.RequestUri);
                throw;
            }
        }

        /// <summary>
        /// Agrega los headers de autenticación a la solicitud
        /// </summary>
        private async Task AgregarHeadersAutenticacionAsync(HttpRequestMessage request)
        {
            try
            {
                // Agregar headers para acceso a la API
                request.Headers.Add("Sitio", "GeoTrack");
                request.Headers.Add("Clave", "GeoTrack2025");

                // Agregar token JWT si existe
                var token = await _localStorage.GetItemAsync<string>("authToken");
                var usuarioId = await _localStorage.GetItemAsync<string>("usuarioId");

                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(usuarioId))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    request.Headers.Add("UsuarioId", usuarioId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar headers de autenticación");
            }
        }
    }
}
