using AutoMapper;
using EquiposAPI.Data;
using EquiposAPI.DTOs.Prestamos;
using EquiposAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EquiposAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrestamosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public PrestamosController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PrestamoDto>>> GetAll([FromQuery] bool soloVencidos = false)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId))
        {
            return Unauthorized();
        }

        var query = _context.Prestamos
            .Include(p => p.Equipo)
            .Include(p => p.Usuario)
            .AsNoTracking()
            .AsQueryable();

        // Regla: "El Estudiante solo ve los propios. El Id se toma del token, nunca por parámetro."
        if (role == "Estudiante")
        {
            query = query.Where(p => p.UsuarioId == currentUserId);
        }

        // Filtro solo vencidos
        if (soloVencidos)
        {
            var hoy = DateTime.UtcNow.Date;
            query = query.Where(p => p.FechaDevolucion == null && p.FechaVencimiento.Date < hoy);
        }

        var prestamos = await query
            .OrderByDescending(p => p.FechaPrestamo)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<PrestamoDto>>(prestamos));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PrestamoDto>> GetById(int id)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId))
        {
            return Unauthorized();
        }

        var prestamo = await _context.Prestamos
            .Include(p => p.Equipo)
            .Include(p => p.Usuario)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (prestamo == null)
        {
            return NotFound(new { mensaje = $"Préstamo con Id {id} no encontrado." });
        }

        if (role == "Estudiante" && prestamo.UsuarioId != currentUserId)
        {
            return Forbid();
        }

        return Ok(_mapper.Map<PrestamoDto>(prestamo));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Docente")]
    public async Task<ActionResult<PrestamoDto>> Create([FromBody] PrestamoCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var now = DateTime.UtcNow;
        var hoy = now.Date;
        var fechaVenc = dto.FechaVencimiento.Date;

        // Regla de negocio: "La FechaVencimiento no puede ser anterior a hoy ni mayor a 7 días desde la fecha del préstamo."
        if (fechaVenc < hoy)
        {
            return BadRequest(new
            {
                mensaje = "La fecha de vencimiento no puede ser anterior a la fecha actual."
            });
        }

        if (fechaVenc > hoy.AddDays(7))
        {
            return BadRequest(new
            {
                mensaje = "La fecha de vencimiento no puede ser superior a 7 días a partir de hoy (fecha de préstamo)."
            });
        }

        // Obtener el equipo a prestar
        var equipo = await _context.Equipos.FirstOrDefaultAsync(e => e.Id == dto.EquipoId);
        if (equipo == null)
        {
            return NotFound(new { mensaje = $"Equipo con Id {dto.EquipoId} no encontrado." });
        }

        // Regla de negocio: "Solo se puede prestar un equipo cuyo Estado sea Disponible. Si no lo está, la API responde 409 Conflict con un mensaje claro."
        if (!string.Equals(equipo.Estado, "Disponible", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new
            {
                mensaje = $"No se puede prestar el equipo '{equipo.Nombre}' ({equipo.CodigoInventario}) porque su estado actual es '{equipo.Estado}'."
            });
        }

        // Determinar el usuario asignado
        int usuarioAsignadoId;
        if (dto.UsuarioId.HasValue && dto.UsuarioId.Value > 0)
        {
            var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == dto.UsuarioId.Value);
            if (!usuarioExiste)
            {
                return BadRequest(new { mensaje = $"El usuario con Id {dto.UsuarioId.Value} no existe." });
            }
            usuarioAsignadoId = dto.UsuarioId.Value;
        }
        else
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out usuarioAsignadoId))
            {
                return Unauthorized();
            }
        }

        // Regla de negocio: "Al registrar un préstamo, el Estado del equipo pasa a Prestado en la misma transacción (un solo SaveChanges)."
        equipo.Estado = "Prestado";

        var prestamo = new Prestamo
        {
            EquipoId = equipo.Id,
            UsuarioId = usuarioAsignadoId,
            FechaPrestamo = now,
            FechaVencimiento = dto.FechaVencimiento,
            FechaDevolucion = null,
            Observaciones = dto.Observaciones
        };

        _context.Prestamos.Add(prestamo);

        // Un solo SaveChanges garantiza atomicidad transaccional
        await _context.SaveChangesAsync();

        // Cargar relaciones para devolver DTO
        await _context.Entry(prestamo).Reference(p => p.Equipo).LoadAsync();
        await _context.Entry(prestamo).Reference(p => p.Usuario).LoadAsync();

        var resultDto = _mapper.Map<PrestamoDto>(prestamo);
        return CreatedAtAction(nameof(GetById), new { id = prestamo.Id }, resultDto);
    }

    [HttpPut("{id}/devolucion")]
    [Authorize(Roles = "Admin,Docente")]
    public async Task<IActionResult> RegistrarDevolucion(int id, [FromBody] PrestamoDevolucionDto? dto)
    {
        var prestamo = await _context.Prestamos
            .Include(p => p.Equipo)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (prestamo == null)
        {
            return NotFound(new { mensaje = $"Préstamo con Id {id} no encontrado." });
        }

        if (prestamo.FechaDevolucion.HasValue)
        {
            return BadRequest(new { mensaje = "Este préstamo ya fue registrado como devuelto anteriormente." });
        }

        // Regla de negocio: "Al registrar la devolución se llena FechaDevolucion y el equipo vuelve a Disponible."
        prestamo.FechaDevolucion = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(dto?.Observaciones))
        {
            prestamo.Observaciones = string.IsNullOrWhiteSpace(prestamo.Observaciones)
                ? dto.Observaciones
                : $"{prestamo.Observaciones} | Dev: {dto.Observaciones}";
        }

        if (prestamo.Equipo != null)
        {
            prestamo.Equipo.Estado = "Disponible";
        }

        await _context.SaveChangesAsync();

        await _context.Entry(prestamo).Reference(p => p.Usuario).LoadAsync();
        return Ok(_mapper.Map<PrestamoDto>(prestamo));
    }
}
