using GeoTrack.WEB;
using GeoTrack.WEB.Helpers;
using GeoTrack.WEB.Services.Api;
using GeoTrack.WEB.Services.Interface;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ────────────────────────────────────────────────────────────
// 1)  URL base de la API
// ────────────────────────────────────────────────────────────
var apiUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl")
            ?? "https://localhost:7243/api/";

// ────────────────────────────────────────────────────────────
// 2)  Servicios auxiliares y autenticación
// ────────────────────────────────────────────────────────────
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
        sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();

// ────────────────────────────────────────────────────────────
// 3)  Interceptor HTTP (transient!) y clientes tipados
// ────────────────────────────────────────────────────────────
builder.Services.AddTransient<HttpInterceptor>();

builder.Services.AddHttpClient<IAuthService, ApiAuthService>(c =>
{
    c.BaseAddress = new Uri(apiUrl);
}).AddHttpMessageHandler<HttpInterceptor>();

builder.Services.AddHttpClient<IPaisService, ApiPaisService>(c =>
{
    c.BaseAddress = new Uri(apiUrl);
}).AddHttpMessageHandler<HttpInterceptor>();

builder.Services.AddHttpClient<IDepartamentoService, ApiDepartamentoService>(c =>
{
    c.BaseAddress = new Uri(apiUrl);
}).AddHttpMessageHandler<HttpInterceptor>();

builder.Services.AddHttpClient<ICiudadService, ApiCiudadService>(c =>
{
    c.BaseAddress = new Uri(apiUrl);
}).AddHttpMessageHandler<HttpInterceptor>();

builder.Services.AddHttpClient<IRolService, ApiRolService>(c =>
{
    c.BaseAddress = new Uri(apiUrl);
}).AddHttpMessageHandler<HttpInterceptor>();

// ────────────────────────────────────────────────────────────
await builder.Build().RunAsync();
