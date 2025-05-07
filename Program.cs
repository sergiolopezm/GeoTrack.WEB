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

// Configuraci�n de la URL base de la API
// Obt�n la URL de la API desde la configuraci�n
var apiUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7295/api/";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl) });

// Servicios de almacenamiento y autenticaci�n
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();

// Registro de servicios de API
builder.Services.AddScoped<IAuthService, ApiAuthService>();
builder.Services.AddScoped<IPaisService, ApiPaisService>();
builder.Services.AddScoped<IDepartamentoService, ApiDepartamentoService>();
builder.Services.AddScoped<ICiudadService, ApiCiudadService>();
builder.Services.AddScoped<IRolService, ApiRolService>();

// Configuraci�n de HTTP Interceptor
builder.Services.AddScoped<HttpInterceptor>();
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(apiUrl);
}).AddHttpMessageHandler<HttpInterceptor>();

// Servicio HTTP Factory para obtener clientes HTTP con interceptor
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

await builder.Build().RunAsync();