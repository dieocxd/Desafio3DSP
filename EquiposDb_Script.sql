IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022165638_InitialCreate'
)
BEGIN
    CREATE TABLE [Categorias] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Categorias] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022165638_InitialCreate'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [Id] int NOT NULL IDENTITY,
        [Username] nvarchar(50) NOT NULL,
        [PasswordHash] nvarchar(255) NOT NULL,
        [Email] nvarchar(150) NOT NULL,
        [Rol] nvarchar(30) NOT NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022165638_InitialCreate'
)
BEGIN
    CREATE TABLE [Equipos] (
        [Id] int NOT NULL IDENTITY,
        [CodigoInventario] nvarchar(50) NOT NULL,
        [Nombre] nvarchar(150) NOT NULL,
        [Descripcion] nvarchar(500) NOT NULL,
        [CategoriaId] int NOT NULL,
        [Estado] nvarchar(30) NOT NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_Equipos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Equipos_Categorias_CategoriaId] FOREIGN KEY ([CategoriaId]) REFERENCES [Categorias] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022165638_InitialCreate'
)
BEGIN
    CREATE TABLE [Prestamos] (
        [Id] int NOT NULL IDENTITY,
        [EquipoId] int NOT NULL,
        [UsuarioId] int NOT NULL,
        [FechaPrestamo] datetime2 NOT NULL,
        [FechaVencimiento] datetime2 NOT NULL,
        [FechaDevolucion] datetime2 NULL,
        [Observaciones] nvarchar(500) NULL,
        CONSTRAINT [PK_Prestamos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Prestamos_Equipos_EquipoId] FOREIGN KEY ([EquipoId]) REFERENCES [Equipos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Prestamos_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022165638_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[Categorias]'))
        SET IDENTITY_INSERT [Categorias] ON;
    EXEC(N'INSERT INTO [Categorias] ([Id], [Nombre])
    VALUES (1, N''Laptops''),
    (2, N''Proyectores''),
    (3, N''Kits de IoT''),
    (4, N''Tarjetas Arduino''),
    (5, N''Accesorios y Periféricos'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[Categorias]'))
        SET IDENTITY_INSERT [Categorias] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022165638_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Categorias_Nombre] ON [Categorias] ([Nombre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022165638_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Equipos_CategoriaId] ON [Equipos] ([CategoriaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022165638_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Equipos_CodigoInventario] ON [Equipos] ([CodigoInventario]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022165638_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Prestamos_EquipoId] ON [Prestamos] ([EquipoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022165638_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Prestamos_UsuarioId] ON [Prestamos] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022165638_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_Username] ON [Usuarios] ([Username]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022165638_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261022165638_InitialCreate', N'10.0.13');
END;

COMMIT;
GO

