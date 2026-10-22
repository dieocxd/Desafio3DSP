using System.ComponentModel.DataAnnotations;

namespace EquiposAPI.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Rol { get; set; } = "Estudiante"; // Admin, Docente, Estudiante

    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
