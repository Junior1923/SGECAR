using SGECAR.Shared.Models;

namespace SGECAR.Shared.Contracts.Repositories;

public interface IPermisoRepository
{
    // LISTAR TODOS LOS PERMISOS
    Task<List<Permiso>> ListPermissions();

    // OBTENER UN PERMISO POR ID (CON LOS ROLES QUE LO TIENEN)
    Task<Permiso?> GetPermissionById(int permisoId);

    // VERIFICAR SI YA EXISTE UN NOMBRE DE PERMISO
    Task<bool> ExistsPermissionName(string nombre, int? excluirPermisoId = null);

    // CREAR UN NUEVO PERMISO
    Task CreatePermission(Permiso permiso);

    // ELIMINAR UN PERMISO (PRIMERO LO QUITA DE RolPermiso)
    Task DeletePermission(Permiso permiso);

    // GUARDAR CAMBIOS DE ENTIDADES YA RASTREADAS
    Task SaveChanges();

    // OBTENER PERMISOS POR SUS IDS
    Task<List<Permiso>> GetPermissionsByIds(IEnumerable<int> permisoIds);

    // OBTENER PERMISOS DE UN ROL
    Task<List<Permiso>> GetPermissionsByRole(int rolId);

    // VERIFICAR SI UN ROL TIENE UN PERMISO
    Task<bool> HasPermission(int rolId, string permiso);
}
