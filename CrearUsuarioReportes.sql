-- Script para crear el inicio de sesión y usuario 'reportes' con rol exclusivo 'db_datareader'
USE [master];
GO

-- 1. Habilitar autenticación mixta (SQL Server y Windows)
EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'LoginMode', REG_DWORD, 2;
GO

-- 2. Crear inicio de sesión 'reportes' si no existe
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'reportes')
BEGIN
    CREATE LOGIN [reportes] WITH PASSWORD = 'ReportesPass123*!', CHECK_POLICY = OFF, CHECK_EXPIRATION = OFF;
END
GO

-- 3. Crear usuario 'reportes' en EquiposDb y otorgar únicamente el rol db_datareader
USE [EquiposDb];
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'reportes')
BEGIN
    CREATE USER [reportes] FOR LOGIN [reportes];
END
GO

ALTER ROLE [db_datareader] ADD MEMBER [reportes];
GO

-- Asegurar que NO tenga permisos de escritura
DENY INSERT, UPDATE, DELETE TO [reportes];
GO

PRINT 'Usuario reportes configurado exitosamente con permisos de solo lectura (db_datareader).';
