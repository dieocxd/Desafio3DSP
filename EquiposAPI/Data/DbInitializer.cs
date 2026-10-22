using EquiposAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EquiposAPI.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context, IConfiguration configuration, ILogger logger)
    {
        // Asegurar que las migraciones estén aplicadas
        await context.Database.MigrateAsync();

        // Verificar si la tabla de usuarios está vacía
        if (!await context.Usuarios.AnyAsync())
        {
            logger.LogInformation("Tabla Usuarios vacía. Ejecutando seeder para usuario Admin...");

            var adminUsername = configuration["AdminSeed:Username"] ?? "admin";
            var adminEmail = configuration["AdminSeed:Email"] ?? "admin@udb.edu.sv";
            var adminPassword = configuration["AdminSeed:Password"];

            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                logger.LogWarning("No se encontró AdminSeed:Password en configuración. Usando valor por defecto seguro.");
                adminPassword = "Admin123*!Secure";
            }

            var adminUser = new Usuario
            {
                Username = adminUsername,
                Email = adminEmail,
                Rol = "Admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword)
            };

            await context.Usuarios.AddAsync(adminUser);

            // También sembramos usuarios de prueba para Docente y Estudiante para facilitar la evaluación de los casos
            var docente = new Usuario
            {
                Username = "docente1",
                Email = "docente1@udb.edu.sv",
                Rol = "Docente",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Docente123*")
            };

            var estudiante = new Usuario
            {
                Username = "estudiante1",
                Email = "estudiante1@udb.edu.sv",
                Rol = "Estudiante",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Estudiante123*")
            };

            await context.Usuarios.AddRangeAsync(docente, estudiante);
            await context.SaveChangesAsync();

            logger.LogInformation("Usuarios iniciales creados exitosamente (Admin, Docente, Estudiante).");
        }

        // Sembrar algunos equipos iniciales de ejemplo si no existen
        if (!await context.Equipos.AnyAsync())
        {
            logger.LogInformation("Sembrando equipos iniciales de prueba...");
            var equipos = new List<Equipo>
            {
                new Equipo
                {
                    CodigoInventario = "LAP-001",
                    Nombre = "Dell Latitude 5420",
                    Descripcion = "Laptop Intel Core i7 16GB RAM 512GB SSD",
                    CategoriaId = 1,
                    Estado = "Disponible"
                },
                new Equipo
                {
                    CodigoInventario = "LAP-002",
                    Nombre = "Lenovo ThinkPad E14",
                    Descripcion = "Laptop AMD Ryzen 7 16GB RAM",
                    CategoriaId = 1,
                    Estado = "Disponible"
                },
                new Equipo
                {
                    CodigoInventario = "PROY-001",
                    Nombre = "Epson PowerLite X49",
                    Descripcion = "Proyector 3LCD 3600 lúmenes HDMI",
                    CategoriaId = 2,
                    Estado = "Disponible"
                },
                new Equipo
                {
                    CodigoInventario = "IOT-001",
                    Nombre = "Kit IoT ESP32 Avanzado",
                    Descripcion = "Kit con sensores de temperatura, humedad, relays y pantalla OLED",
                    CategoriaId = 3,
                    Estado = "Disponible"
                },
                new Equipo
                {
                    CodigoInventario = "ARD-001",
                    Nombre = "Arduino Uno Rev3",
                    Descripcion = "Placa de microcontrolador ATmega328P",
                    CategoriaId = 4,
                    Estado = "Disponible"
                }
            };

            await context.Equipos.AddRangeAsync(equipos);
            await context.SaveChangesAsync();
            logger.LogInformation("Equipos iniciales creados exitosamente.");
        }
    }
}
