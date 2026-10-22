using System.ComponentModel.DataAnnotations;

namespace EquiposAPI.DTOs.Equipos;

public class EquipoDto
{
    public int Id { get; set; }
    public string CodigoInventario { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty; // Disponible, Prestado, Mantenimiento
    public byte[]? RowVersion { get; set; }
}

public class EquipoCreateDto
{
    [Required(ErrorMessage = "El código de inventario es requerido.")]
    [MaxLength(50)]
    public string CodigoInventario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del equipo es requerido.")]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es requerida.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida.")]
    public int CategoriaId { get; set; }

    [Required]
    [RegularExpression("^(Disponible|Prestado|Mantenimiento)$", ErrorMessage = "El estado debe ser: Disponible, Prestado o Mantenimiento.")]
    public string Estado { get; set; } = "Disponible";
}

public class EquipoUpdateDto
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "El código de inventario es requerido.")]
    [MaxLength(50)]
    public string CodigoInventario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del equipo es requerido.")]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es requerida.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida.")]
    public int CategoriaId { get; set; }

    [Required]
    [RegularExpression("^(Disponible|Prestado|Mantenimiento)$", ErrorMessage = "El estado debe ser: Disponible, Prestado o Mantenimiento.")]
    public string Estado { get; set; } = "Disponible";

    [Required(ErrorMessage = "El token de concurrencia (RowVersion) es obligatorio.")]
    public byte[]? RowVersion { get; set; }
}
