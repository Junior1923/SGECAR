using System.ComponentModel.DataAnnotations;

namespace SGECAR.Shared.Contracts;

// ===================== AUTENTICACIÓN =====================

public class LoginRequest
{
    [Required(ErrorMessage = "Ingrese el usuario.")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese la contraseña.")]
    public string Contrasena { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiraEn { get; set; }

    public string Mensaje { get; set; } = string.Empty;

    public SesionUsuario Sesion { get; set; } = null!;
}

// ===================== RESPUESTAS GENÉRICAS =====================

public class MensajeResponse
{
    public string Mensaje { get; set; } = string.Empty;

    public MensajeResponse()
    {
    }

    public MensajeResponse(string mensaje)
    {
        Mensaje = mensaje;
    }
}

public class PermisoDenegadoResponse
{
    public string Mensaje { get; set; } = string.Empty;

    public string Accion { get; set; } = string.Empty;

    public string? Rol { get; set; }
}

// ===================== PERMISOS =====================

public class PermisoDto
{
    public int PermisoId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    // PERMISO BASE DEL SISTEMA: NO SE PUEDE ELIMINAR NI RENOMBRAR
    public bool EsSistema { get; set; }
}

public class PermisoRequest
{
    [Required(ErrorMessage = "El nombre del permiso es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre del permiso no puede exceder 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres.")]
    public string? Descripcion { get; set; }
}

// ===================== ROLES =====================

public class RolDto
{
    public int RolId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool EsSistema { get; set; }

    public int CantidadUsuarios { get; set; }

    public List<PermisoDto> Permisos { get; set; } = new();
}

public class RolRequest
{
    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre del rol no puede exceder 50 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    public List<int> PermisoIds { get; set; } = new();
}

// ===================== USUARIOS =====================

public class UsuarioDto
{
    public int UsuarioId { get; set; }

    public string Usuario { get; set; } = string.Empty;

    public int RolId { get; set; }

    public string Rol { get; set; } = string.Empty;

    public bool EstadoCuenta { get; set; }

    public bool Bloqueado => !EstadoCuenta;

    public int IntentosFallidos { get; set; }
}

public class UsuarioCrearRequest
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder 50 caracteres.")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Contrasena { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un rol válido.")]
    public int RolId { get; set; }
}

public class UsuarioActualizarRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un rol válido.")]
    public int RolId { get; set; }

    public bool EstadoCuenta { get; set; } = true;

    // OPCIONAL: SI SE ENVÍA VACÍO O NULL SE CONSERVA LA CONTRASEÑA ACTUAL
    public string? NuevaContrasena { get; set; }
}
