using AutoMapper;
using EquiposAPI.Data;
using EquiposAPI.DTOs.Categorias;
using EquiposAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EquiposAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CategoriasController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetAll()
    {
        var categorias = await _context.Categorias
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<CategoriaDto>>(categorias));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoriaDto>> GetById(int id)
    {
        var categoria = await _context.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria == null)
        {
            return NotFound(new { mensaje = $"Categoría con Id {id} no encontrada." });
        }

        return Ok(_mapper.Map<CategoriaDto>(categoria));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoriaDto>> Create([FromBody] CategoriaCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var nombreTrim = dto.Nombre.Trim();
        var exists = await _context.Categorias
            .AnyAsync(c => c.Nombre.ToLower() == nombreTrim.ToLower());

        if (exists)
        {
            return Conflict(new { mensaje = $"Ya existe una categoría con el nombre '{nombreTrim}'." });
        }

        var categoria = _mapper.Map<Categoria>(dto);
        categoria.Nombre = nombreTrim;

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        var resultDto = _mapper.Map<CategoriaDto>(categoria);
        return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, resultDto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoriaUpdateDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { mensaje = "El Id de la URL no coincide con el cuerpo de la solicitud." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null)
        {
            return NotFound(new { mensaje = $"Categoría con Id {id} no encontrada." });
        }

        var nombreTrim = dto.Nombre.Trim();
        var exists = await _context.Categorias
            .AnyAsync(c => c.Nombre.ToLower() == nombreTrim.ToLower() && c.Id != id);

        if (exists)
        {
            return Conflict(new { mensaje = $"Ya existe otra categoría con el nombre '{nombreTrim}'." });
        }

        categoria.Nombre = nombreTrim;
        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<CategoriaDto>(categoria));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await _context.Categorias
            .Include(c => c.Equipos)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria == null)
        {
            return NotFound(new { mensaje = $"Categoría con Id {id} no encontrada." });
        }

        if (categoria.Equipos.Any())
        {
            return Conflict(new { mensaje = "No se puede eliminar la categoría porque tiene equipos asociados." });
        }

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
