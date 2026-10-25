using Microsoft.AspNetCore.Mvc;
using SGECAR.API.Security;
using SGECAR.Shared.Contracts;
using SGECAR.Shared.Contracts.Services;
using SGECAR.Shared.Security;

namespace SGECAR.API.Controllers
{
    [Route("api/usuarios")]
    [RequierePermiso(Acciones.CrearUsuarios)]
    public class UsuariosController : ApiControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly SesionStore _sesionStore;
        private readonly SesionActual _sesion;

        public UsuariosController(IUsuarioService usuarioService, SesionStore sesionStore, SesionActual sesion)
        {
            _usuarioService = usuarioService;
            _sesionStore = sesionStore;
            _sesion = sesion;
        }

        // LISTAR USUARIOS
        [HttpGet]
        [RequierePermiso(Acciones.Consultar)]
        [ProducesResponseType(typeof(List<UsuarioDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar()
        {
            var usuarios = await _usuarioService.ListUsersAsync();
            return Ok(usuarios.Select(ToDto));
        }

        // OBTENER UN USUARIO
        [HttpGet("{id:int}")]
        [RequierePermiso(Acciones.Consultar)]
        [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Obtener(int id)
        {
            var usuario = await _usuarioService.GetUserAsync(id);

            if (usuario == null)
                return NotFound(new MensajeResponse("El usuario no existe."));

            return Ok(ToDto(usuario));
        }

        // CREAR USUARIO
        [HttpPost]
        [RequierePermiso(Acciones.Agregar)]
        [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Crear([FromBody] UsuarioCrearRequest request)
        {
            var resultado = await _usuarioService.CreateUserAsync(request.Usuario, request.Contrasena, request.RolId);

            if (!resultado.Exitoso)
                return Resultado(resultado);

            var usuario = await _usuarioService.GetUserAsync(resultado.Id!.Value);
            return CreatedAtAction(nameof(Obtener), new { id = resultado.Id }, ToDto(usuario!));
        }

        // MODIFICAR USUARIO (ROL, ESTADO Y OPCIONALMENTE CONTRASEÑA)
        [HttpPut("{id:int}")]
        [RequierePermiso(Acciones.Modificar)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Modificar(int id, [FromBody] UsuarioActualizarRequest request)
        {
            var anterior = await _usuarioService.GetUserAsync(id);
            int? rolAnterior = anterior?.RolId;

            var resultado = await _usuarioService.UpdateUserAsync(
                id, request.RolId, request.EstadoCuenta, request.NuevaContrasena, _sesion.UsuarioId!.Value);

            // SI CAMBIÓ EL ROL, SE DESACTIVÓ LA CUENTA O SE CAMBIÓ LA CONTRASEÑA, SE CIERRAN SUS SESIONES ABIERTAS
            bool cerrarSesiones = rolAnterior != request.RolId
                || !request.EstadoCuenta
                || !string.IsNullOrEmpty(request.NuevaContrasena);

            if (resultado.Exitoso && cerrarSesiones && id != _sesion.UsuarioId)
                _sesionStore.CerrarSesionesDeUsuario(id);

            return Resultado(resultado);
        }

        // DESBLOQUEAR CUENTA BLOQUEADA POR INTENTOS FALLIDOS
        [HttpPost("{id:int}/desbloquear")]
        [RequierePermiso(Acciones.Modificar)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Desbloquear(int id)
        {
            return Resultado(await _usuarioService.UnlockUserAsync(id));
        }

        // ELIMINAR USUARIO
        [HttpDelete("{id:int}")]
        [RequierePermiso(Acciones.Eliminar)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado = await _usuarioService.DeleteUserAsync(id, _sesion.UsuarioId!.Value);

            if (resultado.Exitoso)
                _sesionStore.CerrarSesionesDeUsuario(id);

            return Resultado(resultado);
        }
    }
}
