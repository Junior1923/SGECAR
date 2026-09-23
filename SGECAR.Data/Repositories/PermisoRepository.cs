using Microsoft.EntityFrameworkCore;
using SGECAR.Data.Context;
using SGECAR.Shared.Contracts.Repositories;
using SGECAR.Shared.Models;

namespace SGECAR.Data.Repositories;

public class PermisoRepository : IPermisoRepository
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
            .OrderBy(p => p.PermisoId)
            .ToListAsync();
    }

    // OBTENER UN PERMISO POR ID (CON LOS ROLES QUE LO TIENEN)
    public async Task<Permiso?> GetPermissionById(int permisoId)
    {
        return await _context.Permisos
            .Include(p => p.Rols)
            .FirstOrDefaultAsync(p => p.PermisoId == permisoId);
    }

    // VERIFICAR SI YA EXISTE UN NOMBRE DE PERMISO
    public async Task<bool> ExistsPermissionName(string nombre, int? excluirPermisoId = null)
    {
        return await _context.Permisos
            .AnyAsync(p => p.Nombre == nombre && p.PermisoId != excluirPermisoId);
    }

    // CREAR UN NUEVO PERMISO
    public async Task CreatePermission(Permiso permiso)
    {
        _context.Permisos.Add(permiso);
        await _context.SaveChangesAsync();
    }

    // ELIMINAR UN PERMISO (PRIMERO LO QUITA DE RolPermiso)
    public async Task DeletePermission(Permiso permiso)
    {
        permiso.Rols.Clear();
        _context.Permisos.Remove(permiso);
        await _context.SaveChangesAsync();
    }

    // GUARDAR CAMBIOS DE ENTIDADES YA RASTREADAS
    public async Task SaveChanges()
    {
        await _context.SaveChangesAsync();
    }

    // OBTENER PERMISOS POR SUS IDS
    public async Task<List<Permiso>> GetPermissionsByIds(IEnumerable<int> permisoIds)
    {
        var ids = permisoIds.Distinct().ToList();

        return await _context.Permisos
            .Where(p => ids.Contains(p.PermisoId))
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
