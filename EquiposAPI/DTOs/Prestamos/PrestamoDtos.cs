using System.ComponentModel.DataAnnotations;

namespace EquiposAPI.DTOs.Prestamos;

public class PrestamoDto
{
    public int Id { get; set; }
    public int EquipoId { get; set; }
    public string EquipoCodigo { get; set; } = string.Empty;
    public string EquipoNombre { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime? FechaDevolucion { get; set; }
    public string? Observaciones { get; set; }

    public bool EstaDevuelto => FechaDevolucion.HasValue;
    public bool EstaVencido => !FechaDevolucion.HasValue && FechaVencimiento.Date < DateTime.UtcNow.Date;
    public int DiasAtraso => EstaVencido ? (int)(DateTime.UtcNow.Date - FechaVencimiento.Date).TotalDays : 0;
}

public class PrestamoCreateDto
{
    [Required(ErrorMessage = "El equipo es requerido.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un equipo válido.")]
    public int EquipoId { get; set; }

    public int? UsuarioId { get; set; } // Opcional, si no se envía se toma del token o del formulario si es Admin/Docente

    [Required(ErrorMessage = "La fecha de vencimiento es requerida.")]
    public DateTime FechaVencimiento { get; set; }

    [MaxLength(500)]
    public string? Observaciones { get; set; }
}

public class PrestamoDevolucionDto
{
    [MaxLength(500)]
    public string? Observaciones { get; set; }
}
