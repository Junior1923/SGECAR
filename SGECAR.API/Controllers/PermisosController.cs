using Microsoft.AspNetCore.Mvc;
using SGECAR.API.Security;
using SGECAR.Business.Services;
using SGECAR.Shared.Contracts;
using SGECAR.Shared.Security;

namespace SGECAR.API.Controllers
{
    [Route("api/permisos")]
    public class PermisosController : ApiControllerBase
    {
        private readonly PermisoService _permisoService;
        private readonly RolService _rolService;
        private readonly SesionStore _sesionStore;

        public PermisosController(PermisoService permisoService, RolService rolService, SesionStore sesionStore)
        {
            _permisoService = permisoService;
            _rolService = rolService;
            _sesionStore = sesionStore;
        }

        // LISTAR TODOS LOS PERMISOS DEL SISTEMA
        [HttpGet]
        [RequierePermiso(Acciones.Consultar)]
        [ProducesResponseType(typeof(List<PermisoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar()
        {
            var permisos = await _permisoService.ListPermissionsAsync();
            return Ok(permisos.Select(ToDto));
        }

        // OBTENER UN PERMISO
        [HttpGet("{id:int}")]
        [RequierePermiso(Acciones.Consultar)]
        [ProducesResponseType(typeof(PermisoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Obtener(int id)
        {
            var permiso = await _permisoService.GetPermissionAsync(id);

            if (permiso == null)
                return NotFound(new MensajeResponse("El permiso no existe."));

            return Ok(ToDto(permiso));
        }

        // PERMISOS ASIGNADOS A UN ROL
        [HttpGet("rol/{rolId:int}")]
        [RequierePermiso(Acciones.Consultar)]
        [ProducesResponseType(typeof(List<PermisoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> PorRol(int rolId)
        {
            var permisos = await _permisoService.GetPermissionsByRoleAsync(rolId);
            return Ok(permisos.Select(ToDto));
        }

        // CREAR PERMISO (SE ASIGNA AUTOMÁTICAMENTE AL ROL ADMINISTRADOR)
        [HttpPost]
        [RequierePermiso(Acciones.CrearRoles)]
        [RequierePermiso(Acciones.Agregar)]
        [ProducesResponseType(typeof(PermisoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Crear([FromBody] PermisoRequest request)
        {
            var resultado = await _permisoService.CreatePermissionAsync(request.Nombre, request.Descripcion);

            if (!resultado.Exitoso)
                return Resultado(resultado);

            var permiso = await _permisoService.GetPermissionAsync(resultado.Id!.Value);
            await RefrescarSesiones(permiso!.Rols.Select(r => r.RolId));

            return CreatedAtAction(nameof(Obtener), new { id = resultado.Id }, ToDto(permiso));
        }

        // MODIFICAR PERMISO (LOS PERMISOS BASE SOLO PERMITEN CAMBIAR LA DESCRIPCIÓN)
        [HttpPut("{id:int}")]
        [RequierePermiso(Acciones.CrearRoles)]
        [RequierePermiso(Acciones.Modificar)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Modificar(int id, [FromBody] PermisoRequest request)
        {
            var resultado = await _permisoService.UpdatePermissionAsync(id, request.Nombre, request.Descripcion);

            // SI CAMBIÓ EL NOMBRE, LAS SESIONES DE LOS ROLES QUE LO TIENEN DEBEN VER EL NUEVO NOMBRE
            if (resultado.Exitoso)
            {
                var permiso = await _permisoService.GetPermissionAsync(id);
                await RefrescarSesiones(permiso!.Rols.Select(r => r.RolId));
            }

            return Resultado(resultado);
        }

        // ELIMINAR PERMISO (SE QUITA DE TODOS LOS ROLES; LOS PERMISOS BASE NO SE PUEDEN ELIMINAR)
        [HttpDelete("{id:int}")]
        [RequierePermiso(Acciones.CrearRoles)]
        [RequierePermiso(Acciones.Eliminar)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Eliminar(int id)
        {
            var permiso = await _permisoService.GetPermissionAsync(id);
            var rolesAfectados = permiso?.Rols.Select(r => r.RolId).ToList() ?? new List<int>();

            var resultado = await _permisoService.DeletePermissionAsync(id);

            if (resultado.Exitoso)
                await RefrescarSesiones(rolesAfectados);

            return Resultado(resultado);
        }

        // ACTUALIZA LOS PERMISOS EN LAS SESIONES ABIERTAS DE LOS ROLES INDICADOS
        private async Task RefrescarSesiones(IEnumerable<int> rolIds)
        {
            foreach (var rolId in rolIds.Distinct().ToList())
            {
                var rol = await _rolService.GetRoleAsync(rolId);
                if (rol != null)
                    _sesionStore.ActualizarRol(rolId, rol.Nombre, rol.Permisos.Select(p => p.Nombre).ToList());
            }
        }
    }
}
