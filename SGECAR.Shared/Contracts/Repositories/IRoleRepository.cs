using SGECAR.Shared.Models;

namespace SGECAR.Shared.Contracts.Repositories;

public interface IRoleRepository
{
    // LISTAR TODOS LOS ROLES CON SUS PERMISOS Y USUARIOS
    Task<List<Role>> ListRoles();

    // OBTENER UN ROL POR ID (CON SUS PERMISOS)
    Task<Role?> GetRoleById(int rolId);

    // OBTENER UN ROL POR NOMBRE (CON SUS PERMISOS)
    Task<Role?> GetRoleByName(string nombre);

    // VERIFICAR SI YA EXISTE UN NOMBRE DE ROL
    Task<bool> ExistsRoleName(string nombre, int? excluirRolId = null);

    // CONTAR USUARIOS ASIGNADOS A UN ROL
    Task<int> CountUsersByRole(int rolId);

    // CREAR UN NUEVO ROL
    Task CreateRole(Role role);

    // ELIMINAR UN ROL (PRIMERO QUITA SUS PERMISOS DE RolPermiso)
    Task DeleteRole(Role role);

    // GUARDAR CAMBIOS DE ENTIDADES YA RASTREADAS
    Task SaveChanges();
}
