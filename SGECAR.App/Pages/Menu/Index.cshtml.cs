using SGECAR.App.Seguridad;
using SGECAR.Shared.Security;

namespace SGECAR.App.Pages.Menu;

public class IndexModel : PaginaProtegida
{
    public record Modulo(string Nombre, string Descripcion, string Icono, string Pagina, bool Habilitado);

    public List<Modulo> Modulos { get; private set; } = new();

    public void OnGet()
    {
        // Los módulos de la etapa III se agregan a esta lista
        Modulos = new()
        {
            new("Clientes", "Registrar, modificar, eliminar y consultar clientes", "user-check", "/Clientes/Index", Puede(Acciones.Consultar)),
            new("Productos", "Inventario de productos por categoría", "package", "/Productos/Index", Puede(Acciones.Consultar)),
            new("Usuarios", "Crear, modificar y eliminar cuentas de acceso", "users", "/Usuarios/Index", Puede(Acciones.CrearUsuarios)),
            new("Roles y permisos", "Crear roles y asignar permisos", "key", "/Roles/Index", Puede(Acciones.CrearRoles)),
            new("Reportes", "Reporte de usuarios con filtros", "grid", "/Reportes/Usuarios", Puede(Acciones.CrearUsuarios)),
        };
    }
}
