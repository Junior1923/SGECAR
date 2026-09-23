using Microsoft.AspNetCore.Mvc;
using SGECAR.API.Security;
using SGECAR.Business.Services;
using SGECAR.Shared.Contracts;
using SGECAR.Shared.Security;

namespace SGECAR.API.Controllers
{
    [Route("api/permisos")]
    [RequierePermiso(Acciones.Consultar)]
    public class PermisosController : ApiControllerBase
    {
        private readonly PermisoService _permisoService;

        public PermisosController(PermisoService permisoService)
        {
            _permisoService = permisoService;
        }

        // LISTAR TODOS LOS PERMISOS DEL SISTEMA
        [HttpGet]
        [ProducesResponseType(typeof(List<PermisoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar()
        {
            var permisos = await _permisoService.ListPermissionsAsync();
            return Ok(permisos.Select(ToDto));
        }

        // PERMISOS ASIGNADOS A UN ROL
        [HttpGet("rol/{rolId:int}")]
        [ProducesResponseType(typeof(List<PermisoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> PorRol(int rolId)
        {
            var permisos = await _permisoService.GetPermissionsByRoleAsync(rolId);
            return Ok(permisos.Select(ToDto));
        }
    }
}
