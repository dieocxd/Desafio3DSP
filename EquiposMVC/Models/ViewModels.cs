using System.ComponentModel.DataAnnotations;

namespace EquiposMVC.Models;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
}

public class LoginViewModel
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [Display(Name = "Usuario")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [MaxLength(50)]
    [Display(Name = "Usuario")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Correo electrónico no válido.")]
    [MaxLength(150)]
    [Display(Name = "Correo Electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    [Display(Name = "Rol")]
    public string Rol { get; set; } = "Estudiante";
}

public class UsuarioViewModel
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}

public class CategoriaViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la categoría es requerido.")]
    [MaxLength(100)]
    [Display(Name = "Nombre de la Categoría")]
    public string Nombre { get; set; } = string.Empty;
}

public class EquipoViewModel
{
    public int Id { get; set; }

    [Display(Name = "Código")]
    public string CodigoInventario { get; set; } = string.Empty;

    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;

    [Display(Name = "Categoría")]
    public int CategoriaId { get; set; }

    [Display(Name = "Categoría")]
    public string CategoriaNombre { get; set; } = string.Empty;

    [Display(Name = "Estado")]
    public string Estado { get; set; } = "Disponible";

    public byte[]? RowVersion { get; set; }
}

public class EquipoCreateViewModel
{
    [Required(ErrorMessage = "El código de inventario es obligatorio.")]
    [MaxLength(50)]
    [Display(Name = "Código de Inventario")]
    public string CodigoInventario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del equipo es obligatorio.")]
    [MaxLength(150)]
    [Display(Name = "Nombre del Equipo")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es requerida.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una categoría válida.")]
    [Display(Name = "Categoría")]
    public int CategoriaId { get; set; }

    [Required]
    [Display(Name = "Estado Inicial")]
    public string Estado { get; set; } = "Disponible";

    public List<CategoriaViewModel> CategoriasDisponibles { get; set; } = new();
}

public class EquipoEditViewModel
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "El código de inventario es obligatorio.")]
    [MaxLength(50)]
    [Display(Name = "Código de Inventario")]
    public string CodigoInventario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del equipo es obligatorio.")]
    [MaxLength(150)]
    [Display(Name = "Nombre del Equipo")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es requerida.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una categoría válida.")]
    [Display(Name = "Categoría")]
    public int CategoriaId { get; set; }

    [Required]
    [Display(Name = "Estado")]
    public string Estado { get; set; } = "Disponible";

    [Required]
    public byte[]? RowVersion { get; set; }

    public List<CategoriaViewModel> CategoriasDisponibles { get; set; } = new();
}

public class EquipoIndexViewModel
{
    public List<EquipoViewModel> Items { get; set; } = new();
    public string? Texto { get; set; }
    public int? CategoriaId { get; set; }
    public string? Estado { get; set; }
    public List<CategoriaViewModel> Categorias { get; set; } = new();

    // Paginación (10 por página según requisito)
    public int PaginaActual { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int TotalRegistros { get; set; }
    public bool TienePaginaAnterior => PaginaActual > 1;
    public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
}

public class PrestamoViewModel
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

public class PrestamoCreateViewModel
{
    [Required(ErrorMessage = "Debe seleccionar un equipo.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un equipo válido.")]
    [Display(Name = "Equipo a Prestar")]
    public int EquipoId { get; set; }

    [Display(Name = "Usuario Asignado")]
    public int? UsuarioId { get; set; }

    [Required(ErrorMessage = "La fecha de vencimiento es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de Vencimiento")]
    public DateTime FechaVencimiento { get; set; } = DateTime.Today.AddDays(3);

    [MaxLength(500)]
    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }

    public List<EquipoViewModel> EquiposDisponibles { get; set; } = new();
    public List<UsuarioViewModel> UsuariosDisponibles { get; set; } = new();
}

public class PrestamoDevolucionViewModel
{
    public int Id { get; set; }
    public string EquipoCodigo { get; set; } = string.Empty;
    public string EquipoNombre { get; set; } = string.Empty;
    public string UsuarioNombre { get; set; } = string.Empty;
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaVencimiento { get; set; }

    [MaxLength(500)]
    [Display(Name = "Observaciones de la Devolución")]
    public string? Observaciones { get; set; }
}
