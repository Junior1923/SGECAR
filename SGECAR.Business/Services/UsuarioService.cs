using SGECAR.Business.Security;
using SGECAR.Data.Repositories;
using SGECAR.Shared.Contracts;
using SGECAR.Shared.Models;

namespace SGECAR.Business.Services
{
    public class UsuarioService
    {
        private const int LongitudMaximaUsuario = 50;
        private const int LongitudMinimaContrasena = 8;

        private readonly UsuarioRepository _usuarios;
        private readonly RoleRepository _roles;

        public UsuarioService(UsuarioRepository usuarios, RoleRepository roles)
        {
            _usuarios = usuarios;
            _roles = roles;
        }

        public Task<List<Usuario>> ListUsersAsync() => _usuarios.ListUsers();

        public Task<Usuario?> GetUserAsync(int usuarioId) => _usuarios.GetUserById(usuarioId);

        public async Task<OperacionResultado> CreateUserAsync(string? usuario, string? contrasena, int rolId)
        {
            if (string.IsNullOrWhiteSpace(usuario))
                return OperacionResultado.Error("El nombre de usuario es obligatorio.");

            usuario = usuario.Trim();

            if (usuario.Length > LongitudMaximaUsuario)
                return OperacionResultado.Error($"El nombre de usuario no puede exceder {LongitudMaximaUsuario} caracteres.");

            if (await _usuarios.ExistsUsername(usuario))
                return OperacionResultado.Conflicto($"Ya existe el usuario \"{usuario}\".");

            var errorContrasena = ValidarContrasena(contrasena);
            if (errorContrasena != null)
                return OperacionResultado.Error(errorContrasena);

            if (await _roles.GetRoleById(rolId) == null)
                return OperacionResultado.Error("Seleccione un rol válido.");

            var nuevoUsuario = new Usuario
            {
                Usuario1 = usuario,
                ContrasenaHash = PasswordHasher.Hash(contrasena!),
                RolId = rolId,
                IntentosFallidos = 0,
                EstadoCuenta = true
            };

            await _usuarios.CreateUser(nuevoUsuario);

            return OperacionResultado.Ok($"Usuario \"{usuario}\" creado correctamente.", nuevoUsuario.UsuarioId);
        }

        // nuevaContrasena VACÍA = CONSERVAR LA ACTUAL
        public async Task<OperacionResultado> UpdateUserAsync(
            int usuarioId,
            int rolId,
            bool estadoCuenta,
            string? nuevaContrasena,
            int usuarioActualId)
        {
            var user = await _usuarios.GetUserById(usuarioId);

            if (user == null)
                return OperacionResultado.NoEncontrado("El usuario no existe.");

            if (usuarioId == usuarioActualId && (rolId != user.RolId || !estadoCuenta))
                return OperacionResultado.Error("No puede cambiar su propio rol ni desactivar su propia cuenta.");

            if (await _roles.GetRoleById(rolId) == null)
                return OperacionResultado.Error("Seleccione un rol válido.");

            if (!string.IsNullOrEmpty(nuevaContrasena))
            {
                var errorContrasena = ValidarContrasena(nuevaContrasena);
                if (errorContrasena != null)
                    return OperacionResultado.Error(errorContrasena);

                user.ContrasenaHash = PasswordHasher.Hash(nuevaContrasena);
            }

            user.RolId = rolId;

            // AL REACTIVAR LA CUENTA SE REINICIAN LOS INTENTOS FALLIDOS
            if (estadoCuenta && !user.EstadoCuenta)
                user.IntentosFallidos = 0;

            user.EstadoCuenta = estadoCuenta;

            await _usuarios.SaveChanges();

            return OperacionResultado.Ok($"Usuario \"{user.Usuario1}\" actualizado correctamente.");
        }

        // DESBLOQUEAR CUENTA BLOQUEADA POR INTENTOS FALLIDOS
        public async Task<OperacionResultado> UnlockUserAsync(int usuarioId)
        {
            var user = await _usuarios.GetUserById(usuarioId);

            if (user == null)
                return OperacionResultado.NoEncontrado("El usuario no existe.");

            user.IntentosFallidos = 0;
            user.EstadoCuenta = true;

            await _usuarios.SaveChanges();

            return OperacionResultado.Ok($"Usuario \"{user.Usuario1}\" desbloqueado.");
        }

        public async Task<OperacionResultado> DeleteUserAsync(int usuarioId, int usuarioActualId)
        {
            if (usuarioId == usuarioActualId)
                return OperacionResultado.Error("No puede eliminar su propio usuario.");

            var user = await _usuarios.GetUserById(usuarioId);

            if (user == null)
                return OperacionResultado.NoEncontrado("El usuario no existe.");

            await _usuarios.DeleteUser(user);

            return OperacionResultado.Ok($"Usuario \"{user.Usuario1}\" eliminado correctamente.");
        }

        private static string? ValidarContrasena(string? contrasena)
        {
            if (string.IsNullOrEmpty(contrasena) || contrasena.Length < LongitudMinimaContrasena)
                return $"La contraseña debe tener al menos {LongitudMinimaContrasena} caracteres.";

            if (!contrasena.Any(char.IsLetter) || !contrasena.Any(char.IsDigit))
                return "La contraseña debe contener letras y números.";

            return null;
        }
    }
}
