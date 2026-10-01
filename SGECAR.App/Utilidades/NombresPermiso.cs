using SGECAR.Shared.Security;

namespace SGECAR.App.Utilidades;

// Texto para mostrar en pantalla de cada permiso de la API
public static class NombresPermiso
{
    public static string Descripcion(string accion) => accion switch
    {
        Acciones.Agregar => "Agregar",
        Acciones.Modificar => "Modificar",
        Acciones.Eliminar => "Eliminar",
        Acciones.Consultar => "Consultar",
        Acciones.CrearUsuarios => "Crear usuarios",
        Acciones.CrearRoles => "Crear roles",
        _ => accion
    };
}
