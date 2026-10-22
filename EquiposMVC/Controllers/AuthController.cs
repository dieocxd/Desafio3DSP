using EquiposMVC.Models;
using EquiposMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquiposMVC.Controllers;

public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (IsAuthenticated)
        {
            return RedirectToAction("Index", "Equipos");
        }

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

        var result = await _authService.LoginAsync(model);
        if (!result.IsSuccess || result.Data == null)
        {
            model.ErrorMessage = result.ErrorMessage ?? "Credenciales inválidas. Por favor intente nuevamente.";
            return View(model);
        }

        // Guardar token, rol y username en Session según la convención del parcial
        HttpContext.Session.SetString("JwtToken", result.Data.Token);
        HttpContext.Session.SetString("Username", result.Data.Username);
        HttpContext.Session.SetString("UserRole", result.Data.Rol);
        HttpContext.Session.SetString("Email", result.Data.Email);

        TempData["SuccessMessage"] = $"¡Bienvenido/a, {result.Data.Username} ({result.Data.Rol})!";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Equipos");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (!IsAuthenticated)
        {
            return RedirectToAction("Login");
        }

        if (CurrentRole != "Admin")
        {
            return View("AccessDenied");
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!IsAuthenticated)
        {
            return RedirectToAction("Login");
        }

        if (CurrentRole != "Admin")
        {
            return View("AccessDenied");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.RegisterAsync(model);
        if (!result.IsSuccess)
        {
            return HandleApiError(result, model);
        }

        TempData["SuccessMessage"] = $"Usuario '{model.Username}' ({model.Rol}) registrado exitosamente.";
        return RedirectToAction("Index", "Equipos");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["InfoMessage"] = "Ha cerrado sesión correctamente.";
        return RedirectToAction("Login", "Auth");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
