using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SGECAR.Shared.Contracts;

namespace SGECAR.API.Security
{
    // MARCA UN CONTROLADOR O ACCIÓN COMO ACCESIBLE SIN TOKEN (EJ. LOGIN)
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PermitirAnonimoAttribute : Attribute
    {
    }

    // FILTRO GLOBAL: TODO ENDPOINT REQUIERE UN TOKEN VÁLIDO SALVO LOS MARCADOS CON [PermitirAnonimo]
    public class AutenticacionFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            bool anonimo = context.ActionDescriptor.EndpointMetadata.OfType<PermitirAnonimoAttribute>().Any();
            if (anonimo)
                return;

            var sesion = context.HttpContext.RequestServices.GetRequiredService<SesionActual>();

            if (!sesion.EstaAutenticado)
                context.Result = AccesoResultados.NoAutenticado();
        }
    }

    // EXIGE UN PERMISO DEL ROL ACTIVO. SE PUEDE APLICAR VARIAS VECES (SE EXIGEN TODOS).
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequierePermisoAttribute : Attribute, IAuthorizationFilter
    {
        public RequierePermisoAttribute(string accion)
        {
            Accion = accion;
        }

        public string Accion { get; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // SI OTRO FILTRO YA RECHAZÓ LA PETICIÓN NO SE SOBRESCRIBE EL RESULTADO
            if (context.Result != null)
                return;

            var sesion = context.HttpContext.RequestServices.GetRequiredService<SesionActual>();

            if (!sesion.EstaAutenticado)
            {
                context.Result = AccesoResultados.NoAutenticado();
                return;
            }

            if (!sesion.HasPermission(Accion))
                context.Result = AccesoResultados.PermisoInsuficiente(Accion, sesion.Rol);
        }
    }

    public static class AccesoResultados
    {
        public static IActionResult NoAutenticado()
        {
            return new UnauthorizedObjectResult(
                new MensajeResponse("Sesión no válida o expirada. Inicie sesión nuevamente."));
        }

        public static IActionResult PermisoInsuficiente(string accion, string? rol)
        {
            return new ObjectResult(new PermisoDenegadoResponse
            {
                Mensaje = $"Permisos insuficientes: el rol \"{rol}\" no tiene permiso para realizar la acción {accion}.",
                Accion = accion,
                Rol = rol
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
