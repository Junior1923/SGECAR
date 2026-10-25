using SGECAR.Shared.Models;

namespace SGECAR.Shared.Contracts.Services;

public interface IRolService
{
    Task<List<Role>> ListRolesAsync();

    Task<Role?> GetRoleAsync(int rolId);

    Task<OperacionResultado> CreateRoleAsync(string? nombre, IEnumerable<int> permisoIds);

    // permisoIds REEMPLAZA LA LISTA COMPLETA DE PERMISOS DEL ROL
    Task<OperacionResultado> UpdateRoleAsync(int rolId, string? nombre, IEnumerable<int> permisoIds);

    Task<OperacionResultado> DeleteRoleAsync(int rolId);
}
