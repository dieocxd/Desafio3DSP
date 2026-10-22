using EquiposMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace EquiposMVC.Controllers;

public abstract class BaseController : Controller
{
    protected string? CurrentRole => HttpContext.Session.GetString("UserRole");
    protected string? CurrentUsername => HttpContext.Session.GetString("Username");
    protected bool IsAuthenticated => !string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"));

    protected IActionResult HandleApiError<T>(ApiResponse<T> response, object? model = null, string? viewName = null)
    {
        switch (response.StatusCode)
        {
            case 401:
                HttpContext.Session.Clear();
                TempData["ErrorMessage"] = "Su sesión ha expirado. Por favor inicie sesión nuevamente.";
                return RedirectToAction("Login", "Auth");

            case 403:
                return View("AccessDenied");

            case 409:
                ModelState.AddModelError(string.Empty, response.ErrorMessage ?? "Conflicto en la operación.");
                return viewName != null ? View(viewName, model) : View(model);

            case >= 500:
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error en el servidor. Por favor intente más tarde.");
                return viewName != null ? View(viewName, model) : View(model);

            default:
                ModelState.AddModelError(string.Empty, response.ErrorMessage ?? "Error al procesar la solicitud.");
                return viewName != null ? View(viewName, model) : View(model);
        }
    }
}
