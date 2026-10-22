using AutoMapper;
using EquiposAPI.Data;
using EquiposAPI.DTOs.Equipos;
using EquiposAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EquiposAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EquiposController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public EquiposController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EquipoDto>>> GetAll(
        [FromQuery] string? texto,
        [FromQuery] int? categoriaId,
        [FromQuery] string? estado)
    {
        var query = _context.Equipos
            .Include(e => e.Categoria)
            .AsNoTracking()
            .AsQueryable();

        // Filtro por texto (Código, Nombre o Descripción)
        if (!string.IsNullOrWhiteSpace(texto))
        {
            var textoTrim = texto.Trim().ToLower();
            query = query.Where(e =>
                e.Nombre.ToLower().Contains(textoTrim) ||
                e.CodigoInventario.ToLower().Contains(textoTrim) ||
                e.Descripcion.ToLower().Contains(textoTrim));
        }

        // Filtro por categoría
        if (categoriaId.HasValue && categoriaId.Value > 0)
        {
            query = query.Where(e => e.CategoriaId == categoriaId.Value);
        }

        // Filtro por estado (Disponible, Prestado, Mantenimiento)
        if (!string.IsNullOrWhiteSpace(estado))
        {
            var estadoTrim = estado.Trim().ToLower();
            query = query.Where(e => e.Estado.ToLower() == estadoTrim);
        }

        var equipos = await query
            .OrderBy(e => e.Nombre)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<EquipoDto>>(equipos));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EquipoDto>> GetById(int id)
    {
        var equipo = await _context.Equipos
            .Include(e => e.Categoria)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (equipo == null)
        {
            return NotFound(new { mensaje = $"Equipo con Id {id} no encontrado." });
        }

        return Ok(_mapper.Map<EquipoDto>(equipo));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EquipoDto>> Create([FromBody] EquipoCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var codigoTrim = dto.CodigoInventario.Trim();
        var existsCodigo = await _context.Equipos
            .AnyAsync(e => e.CodigoInventario.ToLower() == codigoTrim.ToLower());

        if (existsCodigo)
        {
            return Conflict(new { mensaje = $"El código de inventario '{codigoTrim}' ya está registrado." });
        }

        var categoriaExists = await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId);
        if (!categoriaExists)
        {
            return BadRequest(new { mensaje = $"La categoría con Id {dto.CategoriaId} no existe." });
        }

        var equipo = _mapper.Map<Equipo>(dto);
        equipo.CodigoInventario = codigoTrim;
        equipo.Nombre = dto.Nombre.Trim();

        _context.Equipos.Add(equipo);
        await _context.SaveChangesAsync();

        // Cargar categoría para mapear DTO
        await _context.Entry(equipo).Reference(e => e.Categoria).LoadAsync();

        var resultDto = _mapper.Map<EquipoDto>(equipo);
        return CreatedAtAction(nameof(GetById), new { id = equipo.Id }, resultDto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] EquipoUpdateDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { mensaje = "El Id en la URL no coincide con el cuerpo de la solicitud." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var equipo = await _context.Equipos.FirstOrDefaultAsync(e => e.Id == id);
        if (equipo == null)
        {
            return NotFound(new { mensaje = $"Equipo con Id {id} no encontrado." });
        }

        var codigoTrim = dto.CodigoInventario.Trim();
        var existsCodigo = await _context.Equipos
            .AnyAsync(e => e.CodigoInventario.ToLower() == codigoTrim.ToLower() && e.Id != id);

        if (existsCodigo)
        {
            return Conflict(new { mensaje = $"El código de inventario '{codigoTrim}' ya está en uso por otro equipo." });
        }

        var categoriaExists = await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId);
        if (!categoriaExists)
        {
            return BadRequest(new { mensaje = $"La categoría con Id {dto.CategoriaId} no existe." });
        }

        // Configurar valor original de RowVersion para detección de concurrencia optimista
        if (dto.RowVersion != null)
        {
            _context.Entry(equipo).Property(e => e.RowVersion).OriginalValue = dto.RowVersion;
        }

        equipo.CodigoInventario = codigoTrim;
        equipo.Nombre = dto.Nombre.Trim();
        equipo.Descripcion = dto.Descripcion;
        equipo.CategoriaId = dto.CategoriaId;
        equipo.Estado = dto.Estado;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new
            {
                mensaje = "El equipo fue modificado simultáneamente por otro usuario. Por favor recargue la página para ver los cambios recientes."
            });
        }

        await _context.Entry(equipo).Reference(e => e.Categoria).LoadAsync();
        return Ok(_mapper.Map<EquipoDto>(equipo));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var equipo = await _context.Equipos
            .Include(e => e.Prestamos)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (equipo == null)
        {
            return NotFound(new { mensaje = $"Equipo con Id {id} no encontrado." });
        }

        // Regla: "Un equipo con préstamos históricos no se puede eliminar. La API responde 409."
        if (equipo.Prestamos.Any())
        {
            return Conflict(new
            {
                mensaje = "No es posible eliminar el equipo porque cuenta con un historial de préstamos asociados en el sistema."
            });
        }

        _context.Equipos.Remove(equipo);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
