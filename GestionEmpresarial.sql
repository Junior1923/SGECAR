/* =========================================================
   SISTEMA DE GESTIÓN EMPRESARIAL
   ETAPA I  - LOGIN Y ROLES
   ETAPA II - MANTENIMIENTOS DE CLIENTES Y PRODUCTOS

   Tecnología:
   ASP.NET Core .NET 8
   Entity Framework Core
   SQL Server

   Base de datos:
   GestionEmpresarial
   ========================================================= */

CREATE DATABASE GestionEmpresarial;
GO

USE GestionEmpresarial;
GO

/* =========================================================
   TABLA ROLES
   ========================================================= */

CREATE TABLE Roles
(
    RolId INT IDENTITY(1,1) NOT NULL,

    Nombre NVARCHAR(50) NOT NULL,

    CONSTRAINT PK_Roles
        PRIMARY KEY (RolId),

    CONSTRAINT UQ_Roles_Nombre
        UNIQUE (Nombre)
);
GO

/* =========================================================
    TABLA PERMISOS
   ========================================================= */

CREATE TABLE Permisos
(
    PermisoId INT IDENTITY(1,1) NOT NULL,

    Nombre NVARCHAR(100) NOT NULL,

    Descripcion NVARCHAR(200) NULL,

    CONSTRAINT PK_Permisos
        PRIMARY KEY (PermisoId),

    CONSTRAINT UQ_Permisos_Nombre
        UNIQUE (Nombre)
);
GO

/* =========================================================
   TABLA ROL_PERMISO
   RELACIÓN MUCHOS A MUCHOS
   ========================================================= */


CREATE TABLE RolPermiso
(
    RolId INT NOT NULL,

    PermisoId INT NOT NULL,

    CONSTRAINT PK_RolPermiso
        PRIMARY KEY (RolId, PermisoId),

    CONSTRAINT FK_RolPermiso_Roles
        FOREIGN KEY (RolId)
        REFERENCES Roles(RolId),

    CONSTRAINT FK_RolPermiso_Permisos
        FOREIGN KEY (PermisoId)
        REFERENCES Permisos(PermisoId)
);
GO

/* =========================================================
   TABLA USUARIOS
   ========================================================= */

   CREATE TABLE Usuarios
(
    UsuarioId INT IDENTITY(1,1) NOT NULL,

    Usuario NVARCHAR(50) NOT NULL,

    ContrasenaHash NVARCHAR(128) NOT NULL,

    RolId INT NOT NULL,

    IntentosFallidos INT NOT NULL
        CONSTRAINT DF_Usuarios_IntentosFallidos
        DEFAULT 0,

    EstadoCuenta BIT NOT NULL
        CONSTRAINT DF_Usuarios_EstadoCuenta
        DEFAULT 1,

    CONSTRAINT PK_Usuarios
        PRIMARY KEY (UsuarioId),

    CONSTRAINT UQ_Usuarios_Usuario
        UNIQUE (Usuario),

    CONSTRAINT FK_Usuarios_Rol
        FOREIGN KEY (RolId)
        REFERENCES Roles(RolId),

    CONSTRAINT CK_Usuarios_IntentosFallidos
        CHECK (IntentosFallidos >= 0 AND IntentosFallidos <= 3)
);
GO

/* =========================================================
   INSERTAR ROLES
   ========================================================= */
   INSERT INTO Roles (Nombre)
VALUES
    ('Administrador'),
    ('Supervisor'),
    ('Ejecutor');
GO

/* =========================================================
   INSERTAR PERMISOS
   ========================================================= */

   INSERT INTO Permisos
(
    Nombre,
    Descripcion
)
VALUES
    ('AGREGAR', 'Permite agregar registros'),
    ('MODIFICAR', 'Permite modificar registros'),
    ('ELIMINAR', 'Permite eliminar registros'),
    ('CONSULTAR', 'Permite consultar registros'),
    ('CREAR_USUARIOS', 'Permite crear nuevos usuarios'),
    ('CREAR_ROLES', 'Permite crear nuevos roles');
GO

/* =========================================================
   PERMISOS DEL ADMINISTRADOR
   ========================================================= */

INSERT INTO RolPermiso
(
    RolId,
    PermisoId
)
SELECT
    r.RolId,
    p.PermisoId
FROM Roles r
CROSS JOIN Permisos p
WHERE r.Nombre = 'Administrador';
GO

/* =========================================================
   PERMISOS DEL SUPERVISOR
   ========================================================= */

INSERT INTO RolPermiso
(
    RolId,
    PermisoId
)
SELECT
    r.RolId,
    p.PermisoId
FROM Roles r
INNER JOIN Permisos p
    ON p.Nombre IN
    (
        'CONSULTAR',
        'MODIFICAR'
    )
WHERE r.Nombre = 'Supervisor';
GO


/* =========================================================
   PERMISOS DEL EJECUTOR
   ========================================================= */

INSERT INTO RolPermiso
(
    RolId,
    PermisoId
)
SELECT
    r.RolId,
    p.PermisoId
FROM Roles r
INNER JOIN Permisos p
    ON p.Nombre IN
    (
        'CONSULTAR',
        'AGREGAR'
    )
WHERE r.Nombre = 'Ejecutor';
GO


/* =========================================================
   CREAR USUARIO ADMINISTRADOR
   Contraseña: Admin123*
   ========================================================= */

INSERT INTO Usuarios
(
    Usuario,
    ContrasenaHash,
    RolId
)
VALUES
(
    'admin',

    CONVERT(
        VARCHAR(128),
        HASHBYTES('SHA2_512', 'Admin123*'),
        2
    ),

    (
        SELECT RolId
        FROM Roles
        WHERE Nombre = 'Administrador'
    )
);
GO


/* =========================================================
   CREAR USUARIO SUPERVISOR
   Contraseña: Super123*
   ========================================================= */

INSERT INTO Usuarios
(
    Usuario,
    ContrasenaHash,
    RolId
)
VALUES
(
    'supervisor',

    CONVERT(
        VARCHAR(128),
        HASHBYTES('SHA2_512', 'Super123*'),
        2
    ),

    (
        SELECT RolId
        FROM Roles
        WHERE Nombre = 'Supervisor'
    )
);
GO


/* =========================================================
   CREAR USUARIO EJECUTOR
   Contraseña: Ejecut123*
   ========================================================= */

INSERT INTO Usuarios
(
    Usuario,
    ContrasenaHash,
    RolId
)
VALUES
(
    'ejecutor',

    CONVERT(
        VARCHAR(128),
        HASHBYTES('SHA2_512', 'Ejecut123*'),
        2
    ),

    (
        SELECT RolId
        FROM Roles
        WHERE Nombre = 'Ejecutor'
    )
);
GO


/* =========================================================
   VERIFICAR USUARIOS
   ========================================================= */

SELECT
    u.UsuarioId,
    u.Usuario,
    r.Nombre AS Rol,
    u.IntentosFallidos,
    u.EstadoCuenta
FROM Usuarios u
INNER JOIN Roles r
    ON u.RolId = r.RolId;
GO


/* =========================================================
   VERIFICAR ROLES Y PERMISOS
   ========================================================= */

SELECT
    r.Nombre AS Rol,
    p.Nombre AS Permiso,
    p.Descripcion
FROM Roles r
INNER JOIN RolPermiso rp
    ON r.RolId = rp.RolId
INNER JOIN Permisos p
    ON rp.PermisoId = p.PermisoId
ORDER BY
    r.RolId,
    p.PermisoId;
GO
/* =========================================================
   ETAPA II - MANTENIMIENTOS DE CLIENTES Y PRODUCTOS
   Tablas Clientes, Categorias y Productos con datos de prueba.

   Los módulos usan los permisos genéricos (AGREGAR, MODIFICAR,
   ELIMINAR, CONSULTAR); no se crean permisos por módulo.
   El bloque se puede ejecutar varias veces sin duplicar datos.
   ========================================================= */

USE GestionEmpresarial;
GO

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    /* 1. Tabla Clientes */
    IF OBJECT_ID(N'dbo.Clientes', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Clientes
        (
            ClienteId INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_Clientes PRIMARY KEY,
            Nombre NVARCHAR(100) NOT NULL,
            Apellido NVARCHAR(100) NOT NULL,
            Telefono NVARCHAR(20) NULL,
            Correo NVARCHAR(254) NOT NULL,
            Direccion NVARCHAR(250) NULL,
            CONSTRAINT UQ_Clientes_Correo UNIQUE (Correo),
            CONSTRAINT CK_Clientes_Nombre_NoVacio
                CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0),
            CONSTRAINT CK_Clientes_Apellido_NoVacio
                CHECK (LEN(LTRIM(RTRIM(Apellido))) > 0),
            CONSTRAINT CK_Clientes_Correo_NoVacio
                CHECK (LEN(LTRIM(RTRIM(Correo))) > 0)
        );
    END;

    /* 2. Tabla Categorias */
    IF OBJECT_ID(N'dbo.Categorias', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Categorias
        (
            CategoriaId INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_Categorias PRIMARY KEY,
            Nombre NVARCHAR(100) NOT NULL,
            CONSTRAINT UQ_Categorias_Nombre UNIQUE (Nombre),
            CONSTRAINT CK_Categorias_Nombre_NoVacio
                CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0)
        );
    END;

    /* 3. Tabla Productos */
    IF OBJECT_ID(N'dbo.Productos', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Productos
        (
            ProductoId INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_Productos PRIMARY KEY,
            Codigo NVARCHAR(50) NOT NULL,
            Nombre NVARCHAR(150) NOT NULL,
            Precio DECIMAL(18,2) NOT NULL,
            Stock INT NOT NULL,
            CategoriaId INT NOT NULL,
            CONSTRAINT UQ_Productos_Codigo UNIQUE (Codigo),
            CONSTRAINT FK_Productos_Categorias
                FOREIGN KEY (CategoriaId) REFERENCES dbo.Categorias(CategoriaId),
            CONSTRAINT CK_Productos_Precio_NoNegativo CHECK (Precio >= 0),
            CONSTRAINT CK_Productos_Stock_NoNegativo CHECK (Stock >= 0),
            CONSTRAINT CK_Productos_Codigo_NoVacio
                CHECK (LEN(LTRIM(RTRIM(Codigo))) > 0),
            CONSTRAINT CK_Productos_Nombre_NoVacio
                CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0)
        );
    END;

    /* 4. Índice para búsquedas de productos por categoría */
    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_Productos_CategoriaId'
          AND object_id = OBJECT_ID(N'dbo.Productos')
    )
    BEGIN
        CREATE INDEX IX_Productos_CategoriaId
            ON dbo.Productos(CategoriaId);
    END;

    /* 5. Datos de prueba: categorías */
    IF NOT EXISTS (SELECT 1 FROM dbo.Categorias WHERE Nombre = N'Electrónica')
        INSERT INTO dbo.Categorias (Nombre) VALUES (N'Electrónica');

    IF NOT EXISTS (SELECT 1 FROM dbo.Categorias WHERE Nombre = N'Oficina')
        INSERT INTO dbo.Categorias (Nombre) VALUES (N'Oficina');

    IF NOT EXISTS (SELECT 1 FROM dbo.Categorias WHERE Nombre = N'Accesorios')
        INSERT INTO dbo.Categorias (Nombre) VALUES (N'Accesorios');

    /* 6. Datos de prueba: clientes */
    IF NOT EXISTS (SELECT 1 FROM dbo.Clientes WHERE Correo = N'ana.prueba@example.com')
        INSERT INTO dbo.Clientes (Nombre, Apellido, Telefono, Correo, Direccion)
        VALUES (N'Ana', N'García', N'809-555-0101', N'ana.prueba@example.com', N'Santo Domingo');

    IF NOT EXISTS (SELECT 1 FROM dbo.Clientes WHERE Correo = N'juan.prueba@example.com')
        INSERT INTO dbo.Clientes (Nombre, Apellido, Telefono, Correo, Direccion)
        VALUES (N'Juan', N'Pérez', N'809-555-0102', N'juan.prueba@example.com', N'Santiago');

    IF NOT EXISTS (SELECT 1 FROM dbo.Clientes WHERE Correo = N'maria.prueba@example.com')
        INSERT INTO dbo.Clientes (Nombre, Apellido, Telefono, Correo, Direccion)
        VALUES (N'María', N'Rodríguez', N'809-555-0103', N'maria.prueba@example.com', N'San Cristóbal');

    /* 7. Datos de prueba: productos */
    IF NOT EXISTS (SELECT 1 FROM dbo.Productos WHERE Codigo = N'PROD-001')
        INSERT INTO dbo.Productos (Codigo, Nombre, Precio, Stock, CategoriaId)
        SELECT N'PROD-001', N'Teclado USB', 850.00, 15, CategoriaId
        FROM dbo.Categorias WHERE Nombre = N'Electrónica';

    IF NOT EXISTS (SELECT 1 FROM dbo.Productos WHERE Codigo = N'PROD-002')
        INSERT INTO dbo.Productos (Codigo, Nombre, Precio, Stock, CategoriaId)
        SELECT N'PROD-002', N'Cuaderno', 125.00, 40, CategoriaId
        FROM dbo.Categorias WHERE Nombre = N'Oficina';

    IF NOT EXISTS (SELECT 1 FROM dbo.Productos WHERE Codigo = N'PROD-003')
        INSERT INTO dbo.Productos (Codigo, Nombre, Precio, Stock, CategoriaId)
        SELECT N'PROD-003', N'Mouse inalámbrico', 650.00, 20, CategoriaId
        FROM dbo.Categorias WHERE Nombre = N'Accesorios';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO

/* =========================================================
   VERIFICAR CLIENTES, CATEGORÍAS Y PRODUCTOS
   ========================================================= */

SELECT ClienteId, Nombre, Apellido, Telefono, Correo, Direccion
FROM dbo.Clientes
ORDER BY ClienteId;

SELECT CategoriaId, Nombre
FROM dbo.Categorias
ORDER BY CategoriaId;

SELECT p.ProductoId, p.Codigo, p.Nombre, p.Precio, p.Stock,
       c.Nombre AS Categoria
FROM dbo.Productos AS p
INNER JOIN dbo.Categorias AS c ON c.CategoriaId = p.CategoriaId
ORDER BY p.ProductoId;
GO
