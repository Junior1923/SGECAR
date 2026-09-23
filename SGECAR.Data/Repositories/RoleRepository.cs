using Microsoft.EntityFrameworkCore;
using SGECAR.Data.Context;
using SGECAR.Shared.Contracts.Repositories;
using SGECAR.Shared.Models;

namespace SGECAR.Data.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly GestionEmpresarialContext _context;

    public RoleRepository(GestionEmpresarialContext context)
    {
        _context = context;
    }

    // LISTAR TODOS LOS ROLES CON SUS PERMISOS Y USUARIOS
    public async Task<List<Role>> ListRoles()
    {
        return await _context.Roles
            .Include(r => r.Permisos)
            .Include(r => r.Usuarios)
            .OrderBy(r => r.RolId)
            .ToListAsync();
    }

    // OBTENER UN ROL POR ID (CON SUS PERMISOS)
    public async Task<Role?> GetRoleById(int rolId)
    {
        return await _context.Roles
            .Include(r => r.Permisos)
            .FirstOrDefaultAsync(r => r.RolId == rolId);
    }

    // OBTENER UN ROL POR NOMBRE (CON SUS PERMISOS)
    public async Task<Role?> GetRoleByName(string nombre)
    {
        return await _context.Roles
            .Include(r => r.Permisos)
            .FirstOrDefaultAsync(r => r.Nombre == nombre);
    }

    // VERIFICAR SI YA EXISTE UN NOMBRE DE ROL
    public async Task<bool> ExistsRoleName(string nombre, int? excluirRolId = null)
    {
        return await _context.Roles
            .AnyAsync(r => r.Nombre == nombre && r.RolId != excluirRolId);
    }

    // CONTAR USUARIOS ASIGNADOS A UN ROL
    public async Task<int> CountUsersByRole(int rolId)
    {
        return await _context.Usuarios
            .CountAsync(u => u.RolId == rolId);
    }

    // CREAR UN NUEVO ROL
    public async Task CreateRole(Role role)
    {
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
    }

    // ELIMINAR UN ROL (PRIMERO QUITA SUS PERMISOS DE RolPermiso)
    public async Task DeleteRole(Role role)
    {
        role.Permisos.Clear();
        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
    }

    // GUARDAR CAMBIOS DE ENTIDADES YA RASTREADAS
    public async Task SaveChanges()
    {
        await _context.SaveChangesAsync();
    }
}
