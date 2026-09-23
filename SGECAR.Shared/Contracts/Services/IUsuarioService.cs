using SGECAR.Shared.Models;

namespace SGECAR.Shared.Contracts.Services;

public interface IUsuarioService
{
    Task<List<Usuario>> ListUsersAsync();

    Task<Usuario?> GetUserAsync(int usuarioId);

    Task<OperacionResultado> CreateUserAsync(string? usuario, string? contrasena, int rolId);

    // nuevaContrasena VACÍA = CONSERVAR LA ACTUAL
    Task<OperacionResultado> UpdateUserAsync(int usuarioId, int rolId, bool estadoCuenta, string? nuevaContrasena, int usuarioActualId);

    // DESBLOQUEAR CUENTA BLOQUEADA POR INTENTOS FALLIDOS
    Task<OperacionResultado> UnlockUserAsync(int usuarioId);

    Task<OperacionResultado> DeleteUserAsync(int usuarioId, int usuarioActualId);
}
