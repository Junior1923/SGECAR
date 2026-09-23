using SGECAR.Data.Repositories;
using SGECAR.Shared.Contracts;
using SGECAR.Shared.Models;
using SGECAR.Shared.Security;

namespace SGECAR.Business.Services
{
    public class RolService
    {
        private const int LongitudMaximaNombre = 50;

        private readonly RoleRepository _roles;
        private readonly PermisoRepository _permisos;

        public RolService(RoleRepository roles, PermisoRepository permisos)
        {
            _roles = roles;
            _permisos = permisos;
        }

        public Task<List<Role>> ListRolesAsync() => _roles.ListRoles();

        public Task<Role?> GetRoleAsync(int rolId) => _roles.GetRoleById(rolId);

        public static bool EsRolProtegido(Role role) =>
            string.Equals(role.Nombre, RolesSistema.Administrador, StringComparison.OrdinalIgnoreCase);

        public async Task<OperacionResultado> CreateRoleAsync(string? nombre, IEnumerable<int> permisoIds)
        {
            var error = await ValidarNombre(nombre, null);
            if (error != null)
                return error;

            var role = new Role
            {
                Nombre = nombre!.Trim(),
                Permisos = await _permisos.GetPermissionsByIds(permisoIds)
            };

            await _roles.CreateRole(role);

            return OperacionResultado.Ok($"Rol \"{role.Nombre}\" creado correctamente.", role.RolId);
        }

        public async Task<OperacionResultado> UpdateRoleAsync(int rolId, string? nombre, IEnumerable<int> permisoIds)
        {
            var role = await _roles.GetRoleById(rolId);

            if (role == null)
                return OperacionResultado.NoEncontrado("El rol no existe.");

            if (EsRolProtegido(role))
                return OperacionResultado.Conflicto($"El rol \"{role.Nombre}\" es del sistema y no se puede modificar.");

            var error = await ValidarNombre(nombre, rolId);
            if (error != null)
                return error;

            role.Nombre = nombre!.Trim();

            role.Permisos.Clear();
            foreach (var permiso in await _permisos.GetPermissionsByIds(permisoIds))
                role.Permisos.Add(permiso);

            await _roles.SaveChanges();

            return OperacionResultado.Ok($"Rol \"{role.Nombre}\" actualizado correctamente.");
        }

        public async Task<OperacionResultado> DeleteRoleAsync(int rolId)
        {
            var role = await _roles.GetRoleById(rolId);

            if (role == null)
                return OperacionResultado.NoEncontrado("El rol no existe.");

            if (EsRolProtegido(role))
                return OperacionResultado.Conflicto($"El rol \"{role.Nombre}\" es del sistema y no se puede eliminar.");

            int usuarios = await _roles.CountUsersByRole(rolId);
            if (usuarios > 0)
                return OperacionResultado.Conflicto($"No se puede eliminar el rol \"{role.Nombre}\": tiene {usuarios} usuario(s) asignado(s).");

            await _roles.DeleteRole(role);

            return OperacionResultado.Ok($"Rol \"{role.Nombre}\" eliminado correctamente.");
        }

        private async Task<OperacionResultado?> ValidarNombre(string? nombre, int? rolId)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return OperacionResultado.Error("El nombre del rol es obligatorio.");

            nombre = nombre.Trim();

            if (nombre.Length > LongitudMaximaNombre)
                return OperacionResultado.Error($"El nombre del rol no puede exceder {LongitudMaximaNombre} caracteres.");

            if (await _roles.ExistsRoleName(nombre, rolId))
                return OperacionResultado.Conflicto($"Ya existe un rol con el nombre \"{nombre}\".");

            return null;
        }
    }
}
