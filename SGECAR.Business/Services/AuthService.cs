using SGECAR.Business.Security;
using SGECAR.Data.Repositories;
using SGECAR.Shared.Contracts;

namespace SGECAR.Business.Services
{
    public class AuthService
    {
        // COINCIDE CON CK_Usuarios_IntentosFallidos (0..3)
        public const int MaxIntentosFallidos = 3;

        private const string MensajeCuentaBloqueada =
            "La cuenta está bloqueada por exceder el número de intentos permitidos. Contacte al administrador.";

        private readonly UsuarioRepository _usuarios;

        public AuthService(UsuarioRepository usuarios)
        {
            _usuarios = usuarios;
        }

        public async Task<LoginResultado> LoginAsync(string? usuario, string? contrasena)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrEmpty(contrasena))
            {
                return new LoginResultado
                {
                    Estado = EstadoLogin.DatosIncompletos,
                    Mensaje = "Ingrese usuario y contraseña."
                };
            }

            var user = await _usuarios.GetUserByUsername(usuario.Trim());

            if (user == null)
            {
                return new LoginResultado
                {
                    Estado = EstadoLogin.CredencialesInvalidas,
                    Mensaje = "Usuario o contraseña incorrectos."
                };
            }

            if (!user.EstadoCuenta)
            {
                return new LoginResultado
                {
                    Estado = EstadoLogin.CuentaBloqueada,
                    Mensaje = MensajeCuentaBloqueada
                };
            }

            if (!PasswordHasher.Verify(contrasena, user.ContrasenaHash, out bool requiereRehash))
            {
                // REGISTRAR INTENTO FALLIDO Y BLOQUEAR AL LLEGAR AL MÁXIMO
                user.IntentosFallidos = Math.Min(user.IntentosFallidos + 1, MaxIntentosFallidos);

                if (user.IntentosFallidos >= MaxIntentosFallidos)
                    user.EstadoCuenta = false;

                await _usuarios.SaveChanges();

                if (!user.EstadoCuenta)
                {
                    return new LoginResultado
                    {
                        Estado = EstadoLogin.CuentaBloqueada,
                        Mensaje = $"Contraseña incorrecta. Se alcanzaron {MaxIntentosFallidos} intentos fallidos y la cuenta fue bloqueada. Contacte al administrador."
                    };
                }

                int restantes = MaxIntentosFallidos - user.IntentosFallidos;

                return new LoginResultado
                {
                    Estado = EstadoLogin.CredencialesInvalidas,
                    Mensaje = $"Usuario o contraseña incorrectos. Intentos restantes: {restantes}."
                };
            }

            // LOGIN CORRECTO: REINICIAR INTENTOS Y MIGRAR HASH ANTIGUO SI APLICA
            user.IntentosFallidos = 0;

            if (requiereRehash)
                user.ContrasenaHash = PasswordHasher.Hash(contrasena);

            await _usuarios.SaveChanges();

            return new LoginResultado
            {
                Estado = EstadoLogin.Exitoso,
                Mensaje = $"Bienvenido, {user.Usuario1}.",
                Sesion = new SesionUsuario
                {
                    UsuarioId = user.UsuarioId,
                    Usuario = user.Usuario1,
                    RolId = user.RolId,
                    Rol = user.Rol.Nombre,
                    Permisos = user.Rol.Permisos.Select(p => p.Nombre).ToList(),
                    InicioSesion = DateTime.Now
                }
            };
        }
    }
}
