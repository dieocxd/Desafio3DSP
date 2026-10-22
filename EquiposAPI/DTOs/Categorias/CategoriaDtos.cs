using System.ComponentModel.DataAnnotations;

namespace EquiposAPI.DTOs.Categorias;

public class CategoriaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class CategoriaCreateDto
{
    [Required(ErrorMessage = "El nombre de la categoría es requerido.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
}

public class CategoriaUpdateDto
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la categoría es requerido.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
}
