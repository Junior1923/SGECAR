using System.Text.RegularExpressions;
using SGECAR.Shared.Contracts;
using SGECAR.Shared.Contracts.Repositories;
using SGECAR.Shared.Contracts.Services;
using SGECAR.Shared.Models;
using SGECAR.Shared.Security;

namespace SGECAR.Business.Services
{
    public class PermisoService : IPermisoService
    {
        private const int LongitudMaximaNombre = 100;
        private const int LongitudMaximaDescripcion = 200;

        // NOMBRES EN MAYÚSCULAS, NÚMEROS Y GUION BAJO (EJ. EXPORTAR_REPORTES)
        private static readonly Regex FormatoNombre = new("^[A-Z][A-Z0-9_]*$");

        private readonly IPermisoRepository _permisos;
        private readonly IRoleRepository _roles;

        public PermisoService(IPermisoRepository permisos, IRoleRepository roles)
        {
            _permisos = permisos;
            _roles = roles;
        }

        public Task<List<Permiso>> ListPermissionsAsync() => _permisos.ListPermissions();

        public Task<Permiso?> GetPermissionAsync(int permisoId) => _permisos.GetPermissionById(permisoId);

        public Task<List<Permiso>> GetPermissionsByRoleAsync(int rolId) => _permisos.GetPermissionsByRole(rolId);

        public Task<bool> HasPermissionAsync(int rolId, string permiso) => _permisos.HasPermission(rolId, permiso);

        // CREAR PERMISO: SE ASIGNA AUTOMÁTICAMENTE AL ROL ADMINISTRADOR (QUE TIENE TODOS LOS PERMISOS)
        public async Task<OperacionResultado> CreatePermissionAsync(string? nombre, string? descripcion)
        {
            var error = await Validar(nombre, descripcion, null);
            if (error != null)
                return error;

            var permiso = new Permiso
            {
                Nombre = Normalizar(nombre),
                Descripcion = LimpiarDescripcion(descripcion)
            };

            var administrador = await _roles.GetRoleByName(RolesSistema.Administrador);
            if (administrador != null)
                permiso.Rols.Add(administrador);

            await _permisos.CreatePermission(permiso);

            return OperacionResultado.Ok($"Permiso \"{permiso.Nombre}\" creado correctamente.", permiso.PermisoId);
        }

        public async Task<OperacionResultado> UpdatePermissionAsync(int permisoId, string? nombre, string? descripcion)
        {
            var permiso = await _permisos.GetPermissionById(permisoId);

            if (permiso == null)
                return OperacionResultado.NoEncontrado("El permiso no existe.");

            var error = await Validar(nombre, descripcion, permisoId);
            if (error != null)
                return error;

            string nuevoNombre = Normalizar(nombre);

            if (Acciones.EsPermisoBase(permiso.Nombre) && nuevoNombre != permiso.Nombre)
                return OperacionResultado.Conflicto($"El permiso \"{permiso.Nombre}\" es del sistema y no se puede renombrar; solo se puede cambiar su descripción.");

            permiso.Nombre = nuevoNombre;
            permiso.Descripcion = LimpiarDescripcion(descripcion);

            await _permisos.SaveChanges();

            return OperacionResultado.Ok($"Permiso \"{permiso.Nombre}\" actualizado correctamente.");
        }

        // ELIMINAR PERMISO: TAMBIÉN SE QUITA DE TODOS LOS ROLES QUE LO TENGAN
        public async Task<OperacionResultado> DeletePermissionAsync(int permisoId)
        {
            var permiso = await _permisos.GetPermissionById(permisoId);

            if (permiso == null)
                return OperacionResultado.NoEncontrado("El permiso no existe.");

            if (Acciones.EsPermisoBase(permiso.Nombre))
                return OperacionResultado.Conflicto($"El permiso \"{permiso.Nombre}\" es del sistema y no se puede eliminar.");

            int roles = permiso.Rols.Count;
            await _permisos.DeletePermission(permiso);

            return OperacionResultado.Ok($"Permiso \"{permiso.Nombre}\" eliminado correctamente (se quitó de {roles} rol(es)).");
        }

        private async Task<OperacionResultado?> Validar(string? nombre, string? descripcion, int? permisoId)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return OperacionResultado.Error("El nombre del permiso es obligatorio.");

            string normalizado = Normalizar(nombre);

            if (normalizado.Length > LongitudMaximaNombre)
                return OperacionResultado.Error($"El nombre del permiso no puede exceder {LongitudMaximaNombre} caracteres.");

            if (!FormatoNombre.IsMatch(normalizado))
                return OperacionResultado.Error("El nombre del permiso solo puede tener letras sin acento, números y guion bajo, y debe empezar con una letra (ej. EXPORTAR_REPORTES).");

            if (descripcion != null && descripcion.Trim().Length > LongitudMaximaDescripcion)
                return OperacionResultado.Error($"La descripción no puede exceder {LongitudMaximaDescripcion} caracteres.");

            if (await _permisos.ExistsPermissionName(normalizado, permisoId))
                return OperacionResultado.Conflicto($"Ya existe un permiso con el nombre \"{normalizado}\".");

            return null;
        }

        private static string Normalizar(string? nombre) => (nombre ?? string.Empty).Trim().ToUpperInvariant();

        private static string? LimpiarDescripcion(string? descripcion) =>
            string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
    }
}
