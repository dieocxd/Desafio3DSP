using EquiposMVC.Models;
using EquiposMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquiposMVC.Controllers;

public class CategoriasController : BaseController
{
    private readonly ICategoriasService _categoriasService;

    public CategoriasController(ICategoriasService categoriasService)
    {
        _categoriasService = categoriasService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!IsAuthenticated)
        {
            return RedirectToAction("Login", "Auth");
        }

        if (CurrentRole != "Admin")
        {
            return View("AccessDenied");
        }

        var result = await _categoriasService.GetAllAsync();
        if (!result.IsSuccess)
        {
            return HandleApiError(result);
        }

        return View(result.Data ?? new List<CategoriaViewModel>());
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Auth");
        if (CurrentRole != "Admin") return View("AccessDenied");

        return View(new CategoriaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoriaViewModel model)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Auth");
        if (CurrentRole != "Admin") return View("AccessDenied");

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _categoriasService.CreateAsync(model);
        if (!result.IsSuccess)
        {
            return HandleApiError(result, model);
        }

        TempData["SuccessMessage"] = $"Categoría '{model.Nombre}' creada con éxito.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Auth");
        if (CurrentRole != "Admin") return View("AccessDenied");

        var result = await _categoriasService.GetByIdAsync(id);
        if (!result.IsSuccess || result.Data == null)
        {
            return HandleApiError(result);
        }

        return View(result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoriaViewModel model)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Auth");
        if (CurrentRole != "Admin") return View("AccessDenied");

        if (id != model.Id)
        {
            ModelState.AddModelError(string.Empty, "Inconsistencia de identificadores.");
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _categoriasService.UpdateAsync(id, model);
        if (!result.IsSuccess)
        {
            return HandleApiError(result, model);
        }

        TempData["SuccessMessage"] = "Categoría actualizada exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Auth");
        if (CurrentRole != "Admin") return View("AccessDenied");

        var result = await _categoriasService.GetByIdAsync(id);
        if (!result.IsSuccess || result.Data == null)
        {
            return HandleApiError(result);
        }

        return View(result.Data);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Auth");
        if (CurrentRole != "Admin") return View("AccessDenied");

        var result = await _categoriasService.DeleteAsync(id);
        if (!result.IsSuccess)
        {
            // Cargar de nuevo la categoría para mostrar el error en la vista sin perderla
            var getResult = await _categoriasService.GetByIdAsync(id);
            var model = getResult.Data ?? new CategoriaViewModel { Id = id };
            return HandleApiError(result, model, "Delete");
        }

        TempData["SuccessMessage"] = "Categoría eliminada con éxito.";
        return RedirectToAction(nameof(Index));
    }
}
