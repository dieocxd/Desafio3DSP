using System.ComponentModel.DataAnnotations;

namespace EquiposAPI.Models;

public class Prestamo
{
    public int Id { get; set; }

    public int EquipoId { get; set; }
    public Equipo? Equipo { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public DateTime FechaPrestamo { get; set; } = DateTime.UtcNow;

    public DateTime FechaVencimiento { get; set; }

    public DateTime? FechaDevolucion { get; set; }

    [MaxLength(500)]
    public string? Observaciones { get; set; }
}
