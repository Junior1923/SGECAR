using SGECAR.Shared.Models;

namespace SGECAR.Shared.Contracts.Services;

public interface IPermisoService
{
    Task<List<Permiso>> ListPermissionsAsync();

    Task<Permiso?> GetPermissionAsync(int permisoId);

    Task<List<Permiso>> GetPermissionsByRoleAsync(int rolId);

    Task<bool> HasPermissionAsync(int rolId, string permiso);

    // CREAR PERMISO: SE ASIGNA AUTOMÁTICAMENTE AL ROL ADMINISTRADOR
    Task<OperacionResultado> CreatePermissionAsync(string? nombre, string? descripcion);

    Task<OperacionResultado> UpdatePermissionAsync(int permisoId, string? nombre, string? descripcion);

    // ELIMINAR PERMISO: TAMBIÉN SE QUITA DE TODOS LOS ROLES QUE LO TENGAN
    Task<OperacionResultado> DeletePermissionAsync(int permisoId);
}
