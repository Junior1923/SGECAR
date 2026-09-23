using SGECAR.Data.Repositories;
using SGECAR.Shared.Models;

namespace SGECAR.Business.Services
{
    public class PermisoService
    {
        private readonly PermisoRepository _permisos;

        public PermisoService(PermisoRepository permisos)
        {
            _permisos = permisos;
        }

        public Task<List<Permiso>> ListPermissionsAsync() => _permisos.ListPermissions();

        public Task<List<Permiso>> GetPermissionsByRoleAsync(int rolId) => _permisos.GetPermissionsByRole(rolId);

        public Task<bool> HasPermissionAsync(int rolId, string permiso) => _permisos.HasPermission(rolId, permiso);
    }
}
