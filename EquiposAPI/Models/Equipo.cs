using System.ComponentModel.DataAnnotations;

namespace EquiposAPI.Models;

public class Equipo
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string CodigoInventario { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    [Required]
    [MaxLength(30)]
    public string Estado { get; set; } = "Disponible"; // Disponible, Prestado, Mantenimiento

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
