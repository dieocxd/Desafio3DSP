using EquiposReportes.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Cadena de conexión desde User Secrets / Configuración para usuario de solo lectura
var reportesConn = builder.Configuration.GetConnectionString("ReportesConnection")
    ?? "Server=.;Database=EquiposDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";

builder.Services.AddDbContext<EquiposDbContext>(options =>
{
    options.UseSqlServer(reportesConn);
});

// 2. Cliente HTTP para autenticar contra EquiposAPI
builder.Services.AddHttpClient();

// 3. Manejo de sesiones para el token JWT y el rol del usuario autenticado
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
    pattern: "{controller=Reportes}/{action=Inventario}/{id?}");

app.Run();
