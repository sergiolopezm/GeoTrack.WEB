using GeoTrack.WEB;
using GeoTrack.WEB.Helpers;
using GeoTrack.WEB.Services.Api;
using GeoTrack.WEB.Services.Interface;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Globalization;
using Microsoft.Extensions.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configuración de la cultura para formateo de fechas y números
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("es-CO");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("es-CO");

// URL base de la API
var apiUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl");

// Registro de servicios HTTP
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl ?? "https://localhost:7001/api/") });
builder.Services.AddScoped<HttpInterceptor>();

// Registro de servicios de almacenamiento local y autenticación
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();

// Registro de servicios API
builder.Services.AddScoped<IAuthService, ApiAuthService>();
builder.Services.AddScoped<IPaisService, ApiPaisService>();
builder.Services.AddScoped<IDepartamentoService, ApiDepartamentoService>();
builder.Services.AddScoped<ICiudadService, ApiCiudadService>();
builder.Services.AddScoped<IRolService, ApiRolService>(); // Agregamos el servicio de roles

// Configuración de HttpClient para incluir interceptor que añade el token JWT
builder.Services.AddHttpClient("GeoTrackAPI", client =>
{
    client.BaseAddress = new Uri(apiUrl ?? "https://localhost:7001/api/");
}).AddHttpMessageHandler<HttpInterceptor>();

// Servicio HttpClient con nombre específico para ser inyectado
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient("GeoTrackAPI"));

await builder.Build().RunAsync();