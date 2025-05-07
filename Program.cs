using GeoTrack.WEB;
using GeoTrack.WEB.Helpers;
using GeoTrack.WEB.Services.Api;
using GeoTrack.WEB.Services.Interface;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Agregar HttpClient para manejar recursos estáticos
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }
);

// ────────────────────────────────────────────────────────────
// 1)  Cargar configuración y verificar URL base de la API
// ────────────────────────────────────────────────────────────
Console.WriteLine($"Base Address: {builder.HostEnvironment.BaseAddress}");
Console.WriteLine($"Intentando cargar configuración desde: {builder.HostEnvironment.BaseAddress}appsettings.json");

var apiUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl");
if (string.IsNullOrEmpty(apiUrl))
{
    // Si no se encuentra la configuración, usar valor predeterminado
    apiUrl = "https://localhost:7243/api/";
    Console.WriteLine($"⚠️ No se encontró configuración de API. Usando valor predeterminado: {apiUrl}");
}
else
{
    Console.WriteLine($"✅ URL de API cargada de configuración: {apiUrl}");
}

// Asegurarse que la URL termine con "/"
if (!apiUrl.EndsWith("/"))
{
    apiUrl += "/";
}

// Configurar logging avanzado para mejor depuración
builder.Services.AddLogging(logging =>
{
    logging.SetMinimumLevel(LogLevel.Information);
    logging.AddFilter("Microsoft", LogLevel.Warning);
    logging.AddFilter("System", LogLevel.Warning);
    logging.AddFilter("GeoTrack.WEB", LogLevel.Debug);
});

// ────────────────────────────────────────────────────────────
// 2)  Servicios auxiliares y autenticación
// ────────────────────────────────────────────────────────────
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
        sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();

// ────────────────────────────────────────────────────────────
// 3)  Interceptor HTTP (transient!) y clientes tipados
// ────────────────────────────────────────────────────────────
builder.Services.AddTransient<HttpInterceptor>();

// Registrar un delegado de configuración de HttpClient que se usará en todos los clientes
Action<HttpClient> configureClient = (client) =>
{
    client.BaseAddress = new Uri(apiUrl);
    client.Timeout = TimeSpan.FromSeconds(30); // Timeout global para todas las solicitudes
};

// Agregar manejador de errores global
AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
    Console.Error.WriteLine($"Error no manejado: {e.ExceptionObject}");
    Debug.WriteLine($"Error no manejado: {e.ExceptionObject}");
};

// Configurar clientes tipados con el delegado común
builder.Services.AddHttpClient<IAuthService, ApiAuthService>(configureClient)
                .AddHttpMessageHandler<HttpInterceptor>();

builder.Services.AddHttpClient<IPaisService, ApiPaisService>(configureClient)
                .AddHttpMessageHandler<HttpInterceptor>();

builder.Services.AddHttpClient<IDepartamentoService, ApiDepartamentoService>(configureClient)
                .AddHttpMessageHandler<HttpInterceptor>();

builder.Services.AddHttpClient<ICiudadService, ApiCiudadService>(configureClient)
                .AddHttpMessageHandler<HttpInterceptor>();

builder.Services.AddHttpClient<IRolService, ApiRolService>(configureClient)
                .AddHttpMessageHandler<HttpInterceptor>();

// Registrar la URL de la API como un servicio para usarla en cualquier parte de la aplicación
builder.Services.AddSingleton(new ApiUrlConfig { BaseUrl = apiUrl });

// ────────────────────────────────────────────────────────────
await builder.Build().RunAsync();

// Clase auxiliar para inyectar la URL base
public class ApiUrlConfig
{
    public string BaseUrl { get; set; } = string.Empty;
}