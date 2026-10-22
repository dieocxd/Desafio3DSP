using EquiposMVC.Handlers;
using EquiposMVC.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. URL Base de la API leída de configuración (User Secrets / appsettings)
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5000";

// 2. Registro de IHttpContextAccessor y el Handler para enviar el token JWT automáticamente
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<JwtAuthenticationHandler>();

// 3. Registro de clientes HTTP para cada recurso con DelegatingHandler para el JWT
void ConfigureHttpClient(HttpClient client)
{
    client.BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
}

builder.Services.AddHttpClient<IAuthService, AuthService>(ConfigureHttpClient)
    .AddHttpMessageHandler<JwtAuthenticationHandler>();

builder.Services.AddHttpClient<ICategoriasService, CategoriasService>(ConfigureHttpClient)
    .AddHttpMessageHandler<JwtAuthenticationHandler>();

builder.Services.AddHttpClient<IEquiposService, EquiposService>(ConfigureHttpClient)
    .AddHttpMessageHandler<JwtAuthenticationHandler>();

builder.Services.AddHttpClient<IPrestamosService, PrestamosService>(ConfigureHttpClient)
    .AddHttpMessageHandler<JwtAuthenticationHandler>();

// 4. Configuración de Sesión para almacenar token, rol y usuario
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Equipos}/{action=Index}/{id?}");

app.Run();
