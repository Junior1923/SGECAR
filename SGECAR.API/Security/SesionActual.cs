using SGECAR.Shared.Contracts;

namespace SGECAR.API.Security
{
    // SESIÓN DEL USUARIO QUE HACE LA PETICIÓN ACTUAL (USUARIO Y ROL ACTIVO).
    // SE OBTIENE DEL ENCABEZADO "Authorization: Bearer <token>" DEVUELTO POR POST /api/auth/login.
    public class SesionActual
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SesionStore _store;

        private SesionUsuario? _datos;
        private HashSet<string> _permisos = new(StringComparer.OrdinalIgnoreCase);
        private bool _cargado;

        public SesionActual(IHttpContextAccessor httpContextAccessor, SesionStore store)
        {
            _httpContextAccessor = httpContextAccessor;
            _store = store;
        }

        public string? Token
        {
            get
            {
                var header = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
                const string prefijo = "Bearer ";

                if (string.IsNullOrEmpty(header) || !header.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase))
                    return null;

                var token = header[prefijo.Length..].Trim();
                return token.Length == 0 ? null : token;
            }
        }

        public SesionUsuario? Datos
        {
            get
            {
                if (!_cargado)
                {
                    var token = Token;
                    _datos = token == null ? null : _store.Obtener(token);
                    _permisos = new HashSet<string>(_datos?.Permisos ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
                    _cargado = true;
                }

                return _datos;
            }
        }

        public bool EstaAutenticado => Datos != null;

        public int? UsuarioId => Datos?.UsuarioId;

        public string? Usuario => Datos?.Usuario;

        public int? RolId => Datos?.RolId;

        public string? Rol => Datos?.Rol;

        // DEVUELVE TRUE SI EL ROL ACTIVO TIENE EL PERMISO INDICADO (VER SGECAR.Shared.Security.Acciones)
        public bool HasPermission(string accion)
        {
            return EstaAutenticado && _permisos.Contains(accion);
        }
    }
}
