/* =========================================================
   SISTEMA DE GESTIÓN EMPRESARIAL
   ETAPA I - LOGIN Y ROLES

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