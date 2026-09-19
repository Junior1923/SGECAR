using Microsoft.EntityFrameworkCore;
using SGECAR.Data.Context;
using SGECAR.Shared.Models;

namespace SGECAR.Data.Repositories;

public class PermisoRepository
{
    private readonly GestionEmpresarialContext _context;

    public PermisoRepository(GestionEmpresarialContext context)
    {
        _context = context;
    }

    // LISTAR TODOS LOS PERMISOS
    public async Task<List<Permiso>> ListPermissions()
    {
        return await _context.Permisos
            .ToListAsync();
    }

    // OBTENER PERMISOS DE UN ROL
    public async Task<List<Permiso>> GetPermissionsByRole(int rolId)
    {
        return await _context.Permisos
            .Where(p => p.Rols.Any(r => r.RolId == rolId))
            .ToListAsync();
    }

    // VERIFICAR SI UN ROL TIENE UN PERMISO
    public async Task<bool> HasPermission(
        int rolId,
        string permiso)
    {
        return await _context.Permisos
            .AnyAsync(p =>
                p.Nombre == permiso &&
                p.Rols.Any(r => r.RolId == rolId));
    }
}