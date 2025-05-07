using Microsoft.JSInterop;
using System.Text.Json;

namespace GeoTrack.WEB.Helpers
{
    /// <summary>
    /// Servicio para gestionar el almacenamiento local del navegador
    /// </summary>
    public class LocalStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly JsonSerializerOptions _jsonOptions;

        public LocalStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Obtiene un item del localStorage
        /// </summary>
        public async Task<T?> GetItemAsync<T>(string key)
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);

            if (string.IsNullOrEmpty(json))
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Guarda un item en el localStorage
        /// </summary>
        public async Task SetItemAsync<T>(string key, T value)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, JsonSerializer.Serialize(value, _jsonOptions));
        }

        /// <summary>
        /// Elimina un item del localStorage
        /// </summary>
        public async Task RemoveItemAsync(string key)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }

        /// <summary>
        /// Limpia todo el localStorage
        /// </summary>
        public async Task ClearAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.clear");
        }
    }
}
