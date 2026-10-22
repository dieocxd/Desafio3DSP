using EquiposMVC.Models;
using EquiposMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquiposMVC.Controllers;

public class EquiposController : BaseController
{
    private readonly IEquiposService _equiposService;
    private readonly ICategoriasService _categoriasService;

    public EquiposController(IEquiposService equiposService, ICategoriasService categoriasService)
    {
        _equiposService = equiposService;
        _categoriasService = categoriasService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? texto, int? categoriaId, string? estado, int pagina = 1)
    {
        if (!IsAuthenticated)
        {
            return RedirectToAction("Login", "Auth");
        }

        // Si es Estudiante, por regla de negocio solo ve equipos disponibles
        if (CurrentRole == "Estudiante")
        {
            estado = "Disponible";
        }

        var equiposResult = await _equiposService.GetAllAsync(texto, categoriaId, estado);
        if (!equiposResult.IsSuccess)
        {
            return HandleApiError(equiposResult);
        }

        var categoriasResult = await _categoriasService.GetAllAsync();
        var listaEquipos = equiposResult.Data ?? new List<EquipoViewModel>();

        // Paginación obligatoria de 10 registros por página según la rúbrica
        const int pageSize = 10;
        var totalRegistros = listaEquipos.Count;
        var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize);
        if (totalPaginas < 1) totalPaginas = 1;
        if (pagina < 1) pagina = 1;
        if (pagina > totalPaginas) pagina = totalPaginas;

        var itemsPaginados = listaEquipos
            .Skip((pagina - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var viewModel = new EquipoIndexViewModel
        {
            Items = itemsPaginados,
            Texto = texto,
            CategoriaId = categoriaId,
            Estado = estado,
            Categorias = categoriasResult.Data ?? new List<CategoriaViewModel>(),
            PaginaActual = pagina,
            TotalPaginas = totalPaginas,
            TotalRegistros = totalRegistros
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Auth");

        var result = await _equiposService.GetByIdAsync(id);
        if (!result.IsSuccess || result.Data == null)
        {
            return HandleApiError(result);
        }

        return View(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Auth");

        // Regla: CRUD de categorías y equipos solo Admin (Caso 4: Estudiante intenta crear un equipo y recibe 403)
        if (CurrentRole != "Admin")
        {
            return View("AccessDenied");
        }

        var categoriasResult = await _categoriasService.GetAllAsync();
        var model = new EquipoCreateViewModel
        {
            CategoriasDisponibles = categoriasResult.Data ?? new List<CategoriaViewModel>()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EquipoCreateViewModel model)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Auth");
        if (CurrentRole != "Admin") return View("AccessDenied");

        var categoriasResult = await _categoriasService.GetAllAsync();
        model.CategoriasDisponibles = categoriasResult.Data ?? new List<CategoriaViewModel>();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _equiposService.CreateAsync(model);
        if (!result.IsSuccess)
        {
            // Muestra mensaje de la API en el formulario sin perder datos
            return HandleApiError(result, model);
        }

        TempData["SuccessMessage"] = $"Equipo '{model.Nombre}' registrado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Auth");
        if (CurrentRole != "Admin") return View("AccessDenied");

        var result = await _equiposService.GetByIdAsync(id);
        if (!result.IsSuccess || result.Data == null)
        {
            return HandleApiError(result);
        }

        var categoriasResult = await _categoriasService.GetAllAsync();

        var editModel = new EquipoEditViewModel
        {
            Id = result.Data.Id,
            CodigoInventario = result.Data.CodigoInventario,
            Nombre = result.Data.Nombre,
            Descripcion = result.Data.Descripcion,
            CategoriaId = result.Data.CategoriaId,
            Estado = result.Data.Estado,
            RowVersion = result.Data.RowVersion, // Cargar token de concurrencia
            CategoriasDisponibles = categoriasResult.Data ?? new List<CategoriaViewModel>()
        };

        return View(editModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EquipoEditViewModel model)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Auth");
        if (CurrentRole != "Admin") return View("AccessDenied");

        var categoriasResult = await _categoriasService.GetAllAsync();
        model.CategoriasDisponibles = categoriasResult.Data ?? new List<CategoriaViewModel>();

        if (id != model.Id)
        {
            ModelState.AddModelError(string.Empty, "Inconsistencia de identificadores.");
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _equiposService.UpdateAsync(id, model);
        if (!result.IsSuccess)
        {
            // En caso de conflicto de concurrencia optimista (409) o error de validación,
            // se muestra el mensaje de la API sin perder lo que el usuario escribió.
            return HandleApiError(result, model);
        }

        TempData["SuccessMessage"] = "Equipo actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Auth");
        if (CurrentRole != "Admin") return View("AccessDenied");

        var result = await _equiposService.GetByIdAsync(id);
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

        var result = await _equiposService.DeleteAsync(id);
        if (!result.IsSuccess)
        {
            // Regla: "Un equipo con préstamos históricos no se puede eliminar. La API responde 409."
            // Se muestra el error en la vista sin perder la información
            var getResult = await _equiposService.GetByIdAsync(id);
            var model = getResult.Data ?? new EquipoViewModel { Id = id };
            return HandleApiError(result, model, "Delete");
        }

        TempData["SuccessMessage"] = "Equipo eliminado exitosamente.";
        return RedirectToAction(nameof(Index));
    }
}
