using System.ComponentModel.DataAnnotations;

namespace EquiposAPI.Models;

public class Categoria
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    public ICollection<Equipo> Equipos { get; set; } = new List<Equipo>();
}
