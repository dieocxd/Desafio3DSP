using EquiposAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EquiposAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Equipo> Equipos => Set<Equipo>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Prestamo> Prestamos => Set<Prestamo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Categoria
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
            entity.HasIndex(c => c.Nombre).IsUnique();
        });

        // Equipo
        modelBuilder.Entity<Equipo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CodigoInventario).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.CodigoInventario).IsUnique();
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Estado).IsRequired().HasMaxLength(30);
            entity.Property(e => e.RowVersion).IsRowVersion();

            entity.HasOne(e => e.Categoria)
                .WithMany(c => c.Equipos)
                .HasForeignKey(e => e.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.Property(u => u.Rol).IsRequired().HasMaxLength(30);
        });

        // Prestamo
        modelBuilder.Entity<Prestamo>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Observaciones).HasMaxLength(500);

            entity.HasOne(p => p.Equipo)
                .WithMany(e => e.Prestamos)
                .HasForeignKey(p => p.EquipoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Usuario)
                .WithMany(u => u.Prestamos)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Semilla para Categoria (HasData)
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Laptops" },
            new Categoria { Id = 2, Nombre = "Proyectores" },
            new Categoria { Id = 3, Nombre = "Kits de IoT" },
            new Categoria { Id = 4, Nombre = "Tarjetas Arduino" },
            new Categoria { Id = 5, Nombre = "Accesorios y Periféricos" }
        );
    }
}
