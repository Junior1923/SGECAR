namespace SGECAR.Shared.Contracts;

// DATOS DEL USUARIO AUTENTICADO QUE SE GUARDAN EN LA SESIÓN
public class SesionUsuario
{
    public int UsuarioId { get; set; }

    public string Usuario { get; set; } = null!;

    public int RolId { get; set; }

    public string Rol { get; set; } = null!;

    public List<string> Permisos { get; set; } = new();

    public DateTime InicioSesion { get; set; }
}
