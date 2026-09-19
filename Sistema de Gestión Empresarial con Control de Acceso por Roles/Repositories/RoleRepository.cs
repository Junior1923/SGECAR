using Microsoft.EntityFrameworkCore;
using Sistema_de_Gestión_Empresarial_con_Control_de_Acceso_por_Roles.Data;
using Sistema_de_Gestión_Empresarial_con_Control_de_Acceso_por_Roles.Models;

namespace Sistema_de_Gestión_Empresarial_con_Control_de_Acceso_por_Roles.Repositories;

public class RoleRepository
{
    private readonly GestionEmpresarialContext _context;

    public RoleRepository(GestionEmpresarialContext context)
    {
        _context = context;
    }

    // LISTAR TODOS LOS ROLES
    public async Task<List<Role>> ListRoles()
    {
        return await _context.Roles
            .ToListAsync();
    }

    // OBTENER UN ROL POR ID
    public async Task<Role?> GetRoleById(int rolId)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.RolId == rolId);
    }

    // CREAR UN NUEVO ROL
    public async Task<bool> CreateRole(string nombre)
    {
        try
        {
            var role = new Role
            {
                Nombre = nombre
            };

            _context.Roles.Add(role);

            await _context.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }
}