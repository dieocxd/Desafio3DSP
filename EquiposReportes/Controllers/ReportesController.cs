using EquiposReportes.Filters;
using EquiposReportes.Models;
using EquiposReportes.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EquiposReportes.Controllers;

[AdminAuthorize]
public class ReportesController : Controller
{
    private readonly EquiposDbContext _context;

    public ReportesController(EquiposDbContext context)
    {
        _context = context;
    }

    // Reporte 1: Inventario por categoría (conteo por categoría y por estado)
    [HttpGet]
    public async Task<IActionResult> Inventario(int? categoriaId)
    {
        var vm = new InventarioCategoriaReportViewModel
        {
            CategoriaId = categoriaId,
            Categorias = await _context.Categorias
                .AsNoTracking()
                .OrderBy(c => c.Nombre)
                .Select(c => new CategoriaSelectItem { Id = c.Id, Nombre = c.Nombre })
                .ToListAsync()
        };

        var query = _context.Categorias
            .AsNoTracking()
            .AsQueryable();

        if (categoriaId.HasValue && categoriaId.Value > 0)
        {
            query = query.Where(c => c.Id == categoriaId.Value);
        }

        vm.Items = await query
            .OrderBy(c => c.Nombre)
            .Select(c => new InventarioCategoriaItem
            {
                CategoriaId = c.Id,
                CategoriaNombre = c.Nombre,
                TotalEquipos = c.Equipos.Count(),
                CantidadDisponibles = c.Equipos.Count(e => e.Estado == "Disponible"),
                CantidadPrestados = c.Equipos.Count(e => e.Estado == "Prestado"),
                CantidadMantenimiento = c.Equipos.Count(e => e.Estado == "Mantenimiento")
            })
            .ToListAsync();

        return View(vm);
    }

    // Reporte 2: Préstamos vencidos (FechaVencimiento < hoy y FechaDevolucion nula, con username y días de atraso)
    [HttpGet]
    public async Task<IActionResult> Vencidos(int? categoriaId)
    {
        var hoy = DateTime.UtcNow.Date;

        var vm = new PrestamosVencidosReportViewModel
        {
            CategoriaId = categoriaId,
            Categorias = await _context.Categorias
                .AsNoTracking()
                .OrderBy(c => c.Nombre)
                .Select(c => new CategoriaSelectItem { Id = c.Id, Nombre = c.Nombre })
                .ToListAsync()
        };

        var query = _context.Prestamos
            .AsNoTracking()
            .Where(p => p.FechaDevolucion == null && p.FechaVencimiento.Date < hoy)
            .AsQueryable();

        if (categoriaId.HasValue && categoriaId.Value > 0)
        {
            query = query.Where(p => p.Equipo.CategoriaId == categoriaId.Value);
        }

        vm.Items = await query
            .OrderBy(p => p.FechaVencimiento)
            .Select(p => new PrestamoVencidoItem
            {
                PrestamoId = p.Id,
                EquipoCodigo = p.Equipo.CodigoInventario,
                EquipoNombre = p.Equipo.Nombre,
                CategoriaNombre = p.Equipo.Categoria.Nombre,
                UsuarioNombre = p.Usuario.Username,
                UsuarioEmail = p.Usuario.Email,
                FechaPrestamo = p.FechaPrestamo,
                FechaVencimiento = p.FechaVencimiento,
                DiasAtraso = EF.Functions.DateDiffDay(p.FechaVencimiento, hoy)
            })
            .ToListAsync();

        return View(vm);
    }

    // Reporte 3: Top 5 equipos más prestados (conteo en rango de fechas)
    [HttpGet]
    public async Task<IActionResult> TopEquipos(DateTime? fechaInicio, DateTime? fechaFin, int? categoriaId)
    {
        var vm = new TopEquiposReportViewModel
        {
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            CategoriaId = categoriaId,
            Categorias = await _context.Categorias
                .AsNoTracking()
                .OrderBy(c => c.Nombre)
                .Select(c => new CategoriaSelectItem { Id = c.Id, Nombre = c.Nombre })
                .ToListAsync()
        };

        var prestamosQuery = _context.Prestamos
            .AsNoTracking()
            .AsQueryable();

        if (fechaInicio.HasValue)
        {
            prestamosQuery = prestamosQuery.Where(p => p.FechaPrestamo >= fechaInicio.Value.Date);
        }

        if (fechaFin.HasValue)
        {
            prestamosQuery = prestamosQuery.Where(p => p.FechaPrestamo <= fechaFin.Value.Date.AddDays(1).AddTicks(-1));
        }

        if (categoriaId.HasValue && categoriaId.Value > 0)
        {
            prestamosQuery = prestamosQuery.Where(p => p.Equipo.CategoriaId == categoriaId.Value);
        }

        var agrupados = await prestamosQuery
            .GroupBy(p => new
            {
                p.EquipoId,
                p.Equipo.CodigoInventario,
                p.Equipo.Nombre,
                CategoriaNombre = p.Equipo.Categoria.Nombre,
                p.Equipo.Estado
            })
            .Select(g => new
            {
                g.Key.CodigoInventario,
                g.Key.Nombre,
                g.Key.CategoriaNombre,
                g.Key.Estado,
                Total = g.Count()
            })
            .OrderByDescending(x => x.Total)
            .Take(5)
            .ToListAsync();

        var puesto = 1;
        vm.Items = agrupados.Select(x => new TopEquipoItem
        {
            Puesto = puesto++,
            EquipoCodigo = x.CodigoInventario,
            EquipoNombre = x.Nombre,
            CategoriaNombre = x.CategoriaNombre,
            TotalPrestamos = x.Total,
            EstadoActual = x.Estado
        }).ToList();

        return View(vm);
    }

    // Reporte 4: Historial por usuario (búsqueda por Username con todos sus préstamos)
    [HttpGet]
    public async Task<IActionResult> HistorialUsuario(string? username)
    {
        var vm = new HistorialUsuarioReportViewModel
        {
            Username = username
        };

        if (string.IsNullOrWhiteSpace(username))
        {
            return View(vm);
        }

        vm.BusquedaRealizada = true;
        var usuario = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.Trim().ToLower());

        if (usuario == null)
        {
            vm.UsuarioExiste = false;
            return View(vm);
        }

        vm.UsuarioExiste = true;
        vm.UsuarioNombre = usuario.Username;
        vm.UsuarioEmail = usuario.Email;
        vm.UsuarioRol = usuario.Rol;

        vm.Items = await _context.Prestamos
            .AsNoTracking()
            .Where(p => p.UsuarioId == usuario.Id)
            .OrderByDescending(p => p.FechaPrestamo)
            .Select(p => new HistorialPrestamoItem
            {
                PrestamoId = p.Id,
                EquipoCodigo = p.Equipo.CodigoInventario,
                EquipoNombre = p.Equipo.Nombre,
                CategoriaNombre = p.Equipo.Categoria.Nombre,
                FechaPrestamo = p.FechaPrestamo,
                FechaVencimiento = p.FechaVencimiento,
                FechaDevolucion = p.FechaDevolucion,
                EstadoPrestamo = p.FechaDevolucion.HasValue
                    ? "Devuelto"
                    : (p.FechaVencimiento.Date < DateTime.UtcNow.Date ? "Vencido" : "Vigente"),
                Observaciones = p.Observaciones
            })
            .ToListAsync();

        return View(vm);
    }
}
