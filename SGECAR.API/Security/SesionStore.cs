using System.Collections.Concurrent;
using System.Security.Cryptography;
using SGECAR.Shared.Contracts;

namespace SGECAR.API.Security
{
    // ALMACÉN EN MEMORIA DE LAS SESIONES ACTIVAS (TOKEN -> USUARIO/ROL/PERMISOS).
    // LAS SESIONES VIVEN MIENTRAS CORRE LA APLICACIÓN; SI LA API SE REINICIA HAY QUE VOLVER A INICIAR SESIÓN.
    public class SesionStore
    {
        public static readonly TimeSpan TiempoInactividad = TimeSpan.FromMinutes(30);

        private readonly ConcurrentDictionary<string, EntradaSesion> _sesiones = new();

        private class EntradaSesion
        {
            public required SesionUsuario Datos { get; set; }

            public DateTime UltimoAcceso { get; set; }
        }

        public string Crear(SesionUsuario datos)
        {
            LimpiarExpiradas();

            string token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            _sesiones[token] = new EntradaSesion { Datos = datos, UltimoAcceso = DateTime.UtcNow };

            return token;
        }

        // DEVUELVE LA SESIÓN SI EL TOKEN ES VÁLIDO Y RENUEVA SU TIEMPO DE INACTIVIDAD
        public SesionUsuario? Obtener(string token)
        {
            if (!_sesiones.TryGetValue(token, out var entrada))
                return null;

            if (DateTime.UtcNow - entrada.UltimoAcceso > TiempoInactividad)
            {
                _sesiones.TryRemove(token, out _);
                return null;
            }

            entrada.UltimoAcceso = DateTime.UtcNow;
            return entrada.Datos;
        }

        public DateTime? ExpiraEn(string token)
        {
            return _sesiones.TryGetValue(token, out var entrada)
                ? entrada.UltimoAcceso + TiempoInactividad
                : null;
        }

        public void Cerrar(string token)
        {
            _sesiones.TryRemove(token, out _);
        }

        // CIERRA TODAS LAS SESIONES DE UN USUARIO (AL ELIMINARLO, DESACTIVARLO O CAMBIARLE EL ROL)
        public void CerrarSesionesDeUsuario(int usuarioId)
        {
            foreach (var par in _sesiones.Where(s => s.Value.Datos.UsuarioId == usuarioId).ToList())
                _sesiones.TryRemove(par.Key, out _);
        }

        // ACTUALIZA NOMBRE Y PERMISOS DEL ROL EN LAS SESIONES ABIERTAS (AL EDITAR UN ROL)
        public void ActualizarRol(int rolId, string nombreRol, List<string> permisos)
        {
            foreach (var entrada in _sesiones.Values.Where(e => e.Datos.RolId == rolId))
            {
                entrada.Datos = new SesionUsuario
                {
                    UsuarioId = entrada.Datos.UsuarioId,
                    Usuario = entrada.Datos.Usuario,
                    RolId = rolId,
                    Rol = nombreRol,
                    Permisos = new List<string>(permisos),
                    InicioSesion = entrada.Datos.InicioSesion
                };
            }
        }

        private void LimpiarExpiradas()
        {
            var limite = DateTime.UtcNow - TiempoInactividad;

            foreach (var par in _sesiones.Where(s => s.Value.UltimoAcceso < limite).ToList())
                _sesiones.TryRemove(par.Key, out _);
        }
    }
}
