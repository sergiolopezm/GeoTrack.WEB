using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace GeoTrack.WEB.Helpers
{
    /// <summary>
    /// Provider para el estado de autenticación JWT
    /// </summary>
    public class JwtAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly LocalStorageService _localStorage;
        private static readonly AuthenticationState _anonimo = new(new ClaimsPrincipal(new ClaimsIdentity()));

        public JwtAuthenticationStateProvider(LocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        /// <summary>
        /// Obtiene el estado de autenticación actual
        /// </summary>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrEmpty(token))
                return _anonimo;

            try
            {
                var identity = ParseClaimsFromJwt(token);
                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }
            catch
            {
                return _anonimo;
            }
        }

        /// <summary>
        /// Notifica el login del usuario
        /// </summary>
        public void NotificarLogin(string token)
        {
            var identity = ParseClaimsFromJwt(token);
            var user = new ClaimsPrincipal(identity);
            var authState = new AuthenticationState(user);
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

        /// <summary>
        /// Notifica el logout del usuario
        /// </summary>
        public void NotificarLogout()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(_anonimo));
        }

        /// <summary>
        /// Extrae los claims del token JWT
        /// </summary>
        private static ClaimsIdentity ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            var claims = new List<Claim>();

            if (keyValuePairs != null)
            {
                keyValuePairs.TryGetValue(ClaimTypes.NameIdentifier, out var idObj);
                keyValuePairs.TryGetValue(ClaimTypes.Name, out var nombreUsuarioObj);
                keyValuePairs.TryGetValue(ClaimTypes.Role, out var rolObj);
                keyValuePairs.TryGetValue("Nombre", out var nombreObj);
                keyValuePairs.TryGetValue("Apellido", out var apellidoObj);
                keyValuePairs.TryGetValue("jti", out var jtiObj);

                if (idObj != null)
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, idObj.ToString() ?? string.Empty));

                if (nombreUsuarioObj != null)
                    claims.Add(new Claim(ClaimTypes.Name, nombreUsuarioObj.ToString() ?? string.Empty));

                if (rolObj != null)
                    claims.Add(new Claim(ClaimTypes.Role, rolObj.ToString() ?? string.Empty));

                if (nombreObj != null)
                    claims.Add(new Claim("Nombre", nombreObj.ToString() ?? string.Empty));

                if (apellidoObj != null)
                    claims.Add(new Claim("Apellido", apellidoObj.ToString() ?? string.Empty));

                if (jtiObj != null)
                    claims.Add(new Claim(JwtRegisteredClaimNames.Jti, jtiObj.ToString() ?? string.Empty));

                keyValuePairs.Remove(ClaimTypes.NameIdentifier);
                keyValuePairs.Remove(ClaimTypes.Name);
                keyValuePairs.Remove(ClaimTypes.Role);
                keyValuePairs.Remove("Nombre");
                keyValuePairs.Remove("Apellido");
                keyValuePairs.Remove("jti");

                // Agregar los demás claims
                claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty)));
            }

            return new ClaimsIdentity(claims, "jwt");
        }

        /// <summary>
        /// Decodifica el base64 del JWT
        /// </summary>
        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }

    // JWT Registered Claim Names
    internal static class JwtRegisteredClaimNames
    {
        public const string Jti = "jti";
    }
}
