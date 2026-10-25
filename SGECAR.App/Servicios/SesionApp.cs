using System.Text.Json;
using SGECAR.Shared.Contracts;

namespace SGECAR.App.Servicios;

// Guarda en la sesión el token de la API y los datos del usuario logueado
public static class SesionApp
{
    private const string ClaveToken = "Token";
    private const string ClaveUsuario = "Usuario";

    public static void Guardar(this ISession sesion, string token, SesionUsuario usuario)
    {
        sesion.SetString(ClaveToken, token);
        sesion.SetString(ClaveUsuario, JsonSerializer.Serialize(usuario));
    }

    public static string? ObtenerToken(this ISession sesion) => sesion.GetString(ClaveToken);

    public static SesionUsuario? ObtenerUsuario(this ISession sesion)
    {
        var json = sesion.GetString(ClaveUsuario);
        return json is null ? null : JsonSerializer.Deserialize<SesionUsuario>(json);
    }

    // Revisa si el rol del usuario tiene el permiso (usar las constantes de Acciones)
    public static bool TienePermiso(this ISession sesion, string accion) =>
        sesion.ObtenerUsuario()?.Permisos.Contains(accion, StringComparer.OrdinalIgnoreCase) ?? false;

    public static void Cerrar(this ISession sesion) => sesion.Clear();
}