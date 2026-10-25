using Microsoft.AspNetCore.Mvc;
using SGECAR.API.Security;
using SGECAR.Shared.Contracts;
using SGECAR.Shared.Contracts.Services;
using SGECAR.Shared.Security;

namespace SGECAR.API.Controllers
{
    [Route("api/roles")]
    [RequierePermiso(Acciones.CrearRoles)]
    public class RolesController : ApiControllerBase
    {
        private readonly IRolService _rolService;
        private readonly SesionStore _sesionStore;

        public RolesController(IRolService rolService, SesionStore sesionStore)
        {
            _rolService = rolService;
            _sesionStore = sesionStore;
        }

        // LISTAR ROLES
        [HttpGet]
        [RequierePermiso(Acciones.Consultar)]
        [ProducesResponseType(typeof(List<RolDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar()
        {
            var roles = await _rolService.ListRolesAsync();
            return Ok(roles.Select(ToDto));
        }

        // OBTENER UN ROL
        [HttpGet("{id:int}")]
        [RequierePermiso(Acciones.Consultar)]
        [ProducesResponseType(typeof(RolDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Obtener(int id)
        {
            var rol = await _rolService.GetRoleAsync(id);

            if (rol == null)
                return NotFound(new MensajeResponse("El rol no existe."));

            return Ok(ToDto(rol));
        }

        // CREAR ROL
        [HttpPost]
        [RequierePermiso(Acciones.Agregar)]
        [ProducesResponseType(typeof(RolDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Crear([FromBody] RolRequest request)
        {
            var resultado = await _rolService.CreateRoleAsync(request.Nombre, request.PermisoIds);

            if (!resultado.Exitoso)
                return Resultado(resultado);

            var rol = await _rolService.GetRoleAsync(resultado.Id!.Value);
            return CreatedAtAction(nameof(Obtener), new { id = resultado.Id }, ToDto(rol!));
        }

        // MODIFICAR ROL (NOMBRE Y PERMISOS)
        [HttpPut("{id:int}")]
        [RequierePermiso(Acciones.Modificar)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Modificar(int id, [FromBody] RolRequest request)
        {
            var resultado = await _rolService.UpdateRoleAsync(id, request.Nombre, request.PermisoIds);

            // LOS USUARIOS CON SESIÓN ABIERTA EN ESTE ROL RECIBEN LOS NUEVOS PERMISOS DE INMEDIATO
            if (resultado.Exitoso)
            {
                var rol = await _rolService.GetRoleAsync(id);
                _sesionStore.ActualizarRol(id, rol!.Nombre, rol.Permisos.Select(p => p.Nombre).ToList());
            }

            return Resultado(resultado);
        }

        // ELIMINAR ROL
        [HttpDelete("{id:int}")]
        [RequierePermiso(Acciones.Eliminar)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Eliminar(int id)
        {
            return Resultado(await _rolService.DeleteRoleAsync(id));
        }
    }
}
