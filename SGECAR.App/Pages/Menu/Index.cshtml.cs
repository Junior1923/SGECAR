using SGECAR.App.Seguridad;
using SGECAR.Shared.Security;

namespace SGECAR.App.Pages.Menu;

public class IndexModel : PaginaProtegida
{
    public record Modulo(string Nombre, string Descripcion, string Icono, string Pagina, bool Habilitado);

    public List<Modulo> Modulos { get; private set; } = new();

    public void OnGet()
    {
        // Los módulos de las etapas II y III se agregan a esta lista
        Modulos = new()
        {
            new("Usuarios", "Crear y consultar cuentas de acceso", "users", "/Usuarios/Index", Puede(Acciones.CrearUsuarios)),
        };
    }
}
