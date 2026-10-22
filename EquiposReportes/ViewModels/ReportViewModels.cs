using System.ComponentModel.DataAnnotations;

namespace EquiposReportes.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "El nombre de usuario es requerido.")]
    [Display(Name = "Usuario")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
}

public class InventarioCategoriaReportViewModel
{
    public int? CategoriaId { get; set; }
    public List<CategoriaSelectItem> Categorias { get; set; } = new();
    public List<InventarioCategoriaItem> Items { get; set; } = new();
    public int TotalEquiposGeneral => Items.Sum(i => i.TotalEquipos);
    public int TotalDisponiblesGeneral => Items.Sum(i => i.CantidadDisponibles);
    public int TotalPrestadosGeneral => Items.Sum(i => i.CantidadPrestados);
    public int TotalMantenimientoGeneral => Items.Sum(i => i.CantidadMantenimiento);
}

public class InventarioCategoriaItem
{
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public int TotalEquipos { get; set; }
    public int CantidadDisponibles { get; set; }
    public int CantidadPrestados { get; set; }
    public int CantidadMantenimiento { get; set; }
}

public class PrestamosVencidosReportViewModel
{
    public int? CategoriaId { get; set; }
    public List<CategoriaSelectItem> Categorias { get; set; } = new();
    public List<PrestamoVencidoItem> Items { get; set; } = new();
    public int TotalVencidos => Items.Count;
}

public class PrestamoVencidoItem
{
    public int PrestamoId { get; set; }
    public string EquipoCodigo { get; set; } = string.Empty;
    public string EquipoNombre { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public string UsuarioNombre { get; set; } = string.Empty;
    public string UsuarioEmail { get; set; } = string.Empty;
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public int DiasAtraso { get; set; }
}

public class TopEquiposReportViewModel
{
    [DataType(DataType.Date)]
    [Display(Name = "Fecha Desde")]
    public DateTime? FechaInicio { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha Hasta")]
    public DateTime? FechaFin { get; set; }

    public int? CategoriaId { get; set; }
    public List<CategoriaSelectItem> Categorias { get; set; } = new();
    public List<TopEquipoItem> Items { get; set; } = new();
}

public class TopEquipoItem
{
    public int Puesto { get; set; }
    public string EquipoCodigo { get; set; } = string.Empty;
    public string EquipoNombre { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public int TotalPrestamos { get; set; }
    public string EstadoActual { get; set; } = string.Empty;
}

public class HistorialUsuarioReportViewModel
{
    [Display(Name = "Nombre de Usuario")]
    public string? Username { get; set; }

    public bool BusquedaRealizada { get; set; }
    public bool UsuarioExiste { get; set; }
    public string? UsuarioNombre { get; set; }
    public string? UsuarioEmail { get; set; }
    public string? UsuarioRol { get; set; }

    public List<HistorialPrestamoItem> Items { get; set; } = new();
    public int TotalPrestamos => Items.Count;
}

public class HistorialPrestamoItem
{
    public int PrestamoId { get; set; }
    public string EquipoCodigo { get; set; } = string.Empty;
    public string EquipoNombre { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime? FechaDevolucion { get; set; }
    public string EstadoPrestamo { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
}

public class CategoriaSelectItem
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
