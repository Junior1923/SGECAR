using Microsoft.AspNetCore.Mvc;
using SGECAR.API.Security;
using SGECAR.Business.Services;
using SGECAR.Shared.Contracts;

namespace SGECAR.API.Controllers
{
    [Route("api/auth")]
    public class AuthController : ApiControllerBase
    {
        private readonly AuthService _authService;
        private readonly SesionStore _sesionStore;
        private readonly SesionActual _sesion;
        private readonly PermisoService _permisoService;

        public AuthController(AuthService authService, SesionStore sesionStore, SesionActual sesion, PermisoService permisoService)
        {
            _authService = authService;
            _sesionStore = sesionStore;
            _sesion = sesion;
            _permisoService = permisoService;
        }

        // INICIAR SESIÓN
        [HttpPost("login")]
        [PermitirAnonimo]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status423Locked)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var resultado = await _authService.LoginAsync(request.Usuario, request.Contrasena);
            var mensaje = new MensajeResponse(resultado.Mensaje);

            switch (resultado.Estado)
            {
                case EstadoLogin.DatosIncompletos:
                    return BadRequest(mensaje);
                case EstadoLogin.CredencialesInvalidas:
                    return Unauthorized(mensaje);
                case EstadoLogin.CuentaBloqueada:
                    return StatusCode(StatusCodes.Status423Locked, mensaje);
            }

            var token = _sesionStore.Crear(resultado.Sesion!);

            return Ok(new LoginResponse
            {
                Token = token,
                ExpiraEn = _sesionStore.ExpiraEn(token)!.Value,
                Mensaje = resultado.Mensaje,
                Sesion = resultado.Sesion!
            });
        }

        // CERRAR SESIÓN (INVALIDA EL TOKEN)
        [HttpPost("logout")]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            _sesionStore.Cerrar(_sesion.Token!);
            return Ok(new MensajeResponse("Sesión cerrada."));
        }

        // DATOS DE LA SESIÓN ACTUAL (USUARIO, ROL Y PERMISOS)
        [HttpGet("sesion")]
        [ProducesResponseType(typeof(SesionUsuario), StatusCodes.Status200OK)]
        public IActionResult Sesion()
        {
            return Ok(_sesion.Datos);
        }

        // MAPA ACCIÓN -> PERMITIDO (TODOS LOS PERMISOS DE LA BD), PARA HABILITAR/DESHABILITAR BOTONES Y MENÚS EN EL CLIENTE
        [HttpGet("permisos")]
        [ProducesResponseType(typeof(Dictionary<string, bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Permisos()
        {
            var permisos = await _permisoService.ListPermissionsAsync();
            return Ok(permisos.ToDictionary(p => p.Nombre, p => _sesion.HasPermission(p.Nombre)));
        }

        // CONSULTA PUNTUAL: ¿EL ROL ACTIVO TIENE PERMISO PARA {accion}?
        [HttpGet("permisos/{accion}")]
        public IActionResult HasPermission(string accion)
        {
            return Ok(new { accion = accion.ToUpperInvariant(), permitido = _sesion.HasPermission(accion) });
        }
    }
}
