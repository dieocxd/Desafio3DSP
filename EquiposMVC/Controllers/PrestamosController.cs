using EquiposMVC.Models;
using EquiposMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquiposMVC.Controllers;

public class PrestamosController : BaseController
{
    private readonly IPrestamosService _prestamosService;
    private readonly IEquiposService _equiposService;
    private readonly IAuthService _authService;

    public PrestamosController(
        IPrestamosService prestamosService,
        IEquiposService equiposService,
        IAuthService authService)
    {
        _prestamosService = prestamosService;
        _equiposService = equiposService;
        _authService = authService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(bool soloVencidos = false)
    {
        if (!IsAuthenticated)
        {
            return RedirectToAction("Login", "Auth");
        }

        var result = await _prestamosService.GetAllAsync(soloVencidos);
        if (!result.IsSuccess)
        {
            return HandleApiError(result);
        }

        ViewBag.SoloVencidos = soloVencidos;
        return View(result.Data ?? new List<PrestamoViewModel>());
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? equipoId)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Auth");

        // Regla: Registrar préstamo y devolución solo Admin y Docente (Estudiante -> 403)
        if (CurrentRole != "Admin" && CurrentRole != "Docente")
        {
            return View("AccessDenied");
        }

        var model = new PrestamoCreateViewModel();
        if (equipoId.HasValue)
        {
            model.EquipoId = equipoId.Value;
        }

        await CargarCatalogosParaPrestamo(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PrestamoCreateViewModel model)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Auth");
        if (CurrentRole != "Admin" && CurrentRole != "Docente") return View("AccessDenied");

        if (!ModelState.IsValid)
        {
            await CargarCatalogosParaPrestamo(model);
            return View(model);
        }

        var result = await _prestamosService.CreateAsync(model);
        if (!result.IsSuccess)
        {
            // Muestra mensaje de la API (ej. 409 equipo no disponible o validación de fecha 7 días) sin perder lo escrito
            await CargarCatalogosParaPrestamo(model);
            return HandleApiError(result, model);
        }

        TempData["SuccessMessage"] = "Préstamo registrado exitosamente y equipo marcado como Prestado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Devolucion(int id)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Auth");
        if (CurrentRole != "Admin" && CurrentRole != "Docente") return View("AccessDenied");

        var result = await _prestamosService.GetByIdAsync(id);
        if (!result.IsSuccess || result.Data == null)
        {
            return HandleApiError(result);
        }

        if (result.Data.EstaDevuelto)
        {
            TempData["InfoMessage"] = "El préstamo ya fue devuelto previamente.";
            return RedirectToAction(nameof(Index));
        }

        var model = new PrestamoDevolucionViewModel
        {
            Id = result.Data.Id,
            EquipoCodigo = result.Data.EquipoCodigo,
            EquipoNombre = result.Data.EquipoNombre,
            UsuarioNombre = result.Data.UsuarioNombre,
            FechaPrestamo = result.Data.FechaPrestamo,
            FechaVencimiento = result.Data.FechaVencimiento,
            Observaciones = result.Data.Observaciones
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Devolucion(int id, PrestamoDevolucionViewModel model)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Auth");
        if (CurrentRole != "Admin" && CurrentRole != "Docente") return View("AccessDenied");

        if (id != model.Id)
        {
            ModelState.AddModelError(string.Empty, "Inconsistencia de identificadores.");
            return View(model);
        }

        var result = await _prestamosService.RegistrarDevolucionAsync(id, model.Observaciones);
        if (!result.IsSuccess)
        {
            return HandleApiError(result, model);
        }

        TempData["SuccessMessage"] = "Devolución registrada exitosamente. El equipo vuelve a estar Disponible.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarCatalogosParaPrestamo(PrestamoCreateViewModel model)
    {
        // Equipos disponibles
        var equiposRes = await _equiposService.GetAllAsync(estado: "Disponible");
        model.EquiposDisponibles = equiposRes.Data ?? new List<EquipoViewModel>();

        // Si es Admin o Docente, puede seleccionar a qué usuario prestarle
        var usuariosRes = await _authService.GetUsuariosAsync();
        model.UsuariosDisponibles = usuariosRes.Data ?? new List<UsuarioViewModel>();
    }
}
