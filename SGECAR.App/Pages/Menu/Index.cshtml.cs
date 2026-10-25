using SGECAR.App.Seguridad;
using SGECAR.Shared.Security;

namespace SGECAR.App.Pages.Menu;

public class IndexModel : PaginaProtegida
{
    public record Modulo(string Nombre, string Descripcion, string Icono, string Pagina, bool Habilitado);

    public List<Modulo> Modulos { get; private set; } = new();

    public void OnGet()
    {
        
        Modulos = new()
        {
            new("Usuarios", "Crear, modificar y eliminar cuentas de acceso", "users", "/Usuarios/Index", Puede(Acciones.CrearUsuarios)),
            new("Roles y permisos", "Crear roles y asignar permisos", "key", "/Roles/Index", Puede(Acciones.CrearRoles)),
        };
    }
}
