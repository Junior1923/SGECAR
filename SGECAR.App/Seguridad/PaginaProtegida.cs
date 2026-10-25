using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SGECAR.App.Servicios;
using SGECAR.App.Utilidades;
using SGECAR.Shared.Contracts;

namespace SGECAR.App.Seguridad;

// Clase base de las páginas que requieren sesión.
// Verifica que haya un usuario logueado y, si la página exige un permiso,
// que su rol lo tenga. Así se bloquea el acceso aunque se escriba la
// dirección directamente en el navegador.
public abstract class PaginaProtegida : PageModel
{
    public SesionUsuario UsuarioActual { get; private set; } = new();

    // Permiso necesario para abrir la página (null = solo requiere sesión)
    protected virtual string? AccionRequerida => null;

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var usuario = HttpContext.Session.ObtenerUsuario();
        if (usuario is null)
        {
            this.MostrarMessageBox(TipoMensaje.Advertencia, "Sesión requerida",
                "Debe iniciar sesión para acceder al sistema.");
            context.Result = RedirectToPage("/Cuenta/Login");
            return;
        }

        UsuarioActual = usuario;

        if (AccionRequerida is not null && !Puede(AccionRequerida))
            context.Result = PermisoDenegado(AccionRequerida);
    }

    public bool Puede(string accion) => HttpContext.Session.TienePermiso(accion);

    protected IActionResult PermisoDenegado(string accion)
    {
        this.MostrarMessageBox(TipoMensaje.Error, "Permisos insuficientes",
            $"Su rol ({UsuarioActual.Rol}) no tiene permiso para: {NombresPermiso.Descripcion(accion)}.");
        return RedirectToPage("/Menu/Index");
    }

    // Revisa los errores comunes de la API. Devuelve una redirección si hay que salir de la página.
    protected IActionResult? ManejarErrorApi<T>(RespuestaApi<T> respuesta)
    {
        if (respuesta.SesionVencida)
        {
            HttpContext.Session.Cerrar();
            this.MostrarMessageBox(TipoMensaje.Advertencia, "Sesión vencida", respuesta.Mensaje);
            return RedirectToPage("/Cuenta/Login");
        }

        if (respuesta.SinPermiso)
        {
            this.MostrarMessageBox(TipoMensaje.Error, "Permisos insuficientes", respuesta.Mensaje);
            return RedirectToPage("/Menu/Index");
        }

        return null;
    }
}
