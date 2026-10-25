using SGECAR.Shared.Models;

namespace SGECAR.Shared.Contracts.Repositories;

public interface IUsuarioRepository
{
    // OBTENER USUARIO POR NOMBRE (INCLUYE ROL Y PERMISOS PARA EL LOGIN)
    Task<Usuario?> GetUserByUsername(string usuario);

    // OBTENER USUARIO POR ID (INCLUYE ROL)
    Task<Usuario?> GetUserById(int usuarioId);

    // LISTAR USUARIOS
    Task<List<Usuario>> ListUsers();

    // VERIFICAR SI YA EXISTE UN NOMBRE DE USUARIO
    Task<bool> ExistsUsername(string usuario, int? excluirUsuarioId = null);

    // CREAR USUARIO
    Task CreateUser(Usuario usuario);

    // ELIMINAR USUARIO
    Task DeleteUser(Usuario usuario);

    // GUARDAR CAMBIOS DE ENTIDADES YA RASTREADAS
    Task SaveChanges();
}
