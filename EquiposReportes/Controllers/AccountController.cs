using System.Text;
using System.Text.Json;
using EquiposReportes.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EquiposReportes.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AccountController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "http://localhost:5000";
            var client = _httpClientFactory.CreateClient();
            var loginPayload = new
            {
                Username = model.Username.Trim(),
                Password = model.Password
            };

            var content = new StringContent(JsonSerializer.Serialize(loginPayload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{apiBaseUrl.TrimEnd('/')}/api/auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                model.ErrorMessage = "Credenciales incorrectas o usuario no encontrado en la API central.";
                return View(model);
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            var token = root.GetProperty("token").GetString();
            var username = root.GetProperty("username").GetString();
            var rol = root.GetProperty("rol").GetString();

            // Regla: "El acceso exige iniciar sesión contra /api/auth/login y solo permite el rol Admin."
            if (!string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                model.ErrorMessage = "Acceso denegado: El módulo de reportes es exclusivo para usuarios con rol 'Admin'.";
                return View(model);
            }

            // Guardar en sesión
            HttpContext.Session.SetString("JwtToken", token ?? "");
            HttpContext.Session.SetString("Username", username ?? "");
            HttpContext.Session.SetString("UserRole", rol ?? "");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Inventario", "Reportes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al autenticar contra EquiposAPI");
            model.ErrorMessage = "No se pudo conectar con el servicio EquiposAPI. Verifique que la API esté en ejecución.";
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
