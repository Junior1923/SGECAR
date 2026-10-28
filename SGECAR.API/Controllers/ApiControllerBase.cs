using Microsoft.AspNetCore.Mvc;
using SGECAR.Shared.Contracts;
using SGECAR.Shared.Models;
using SGECAR.Shared.Security;

namespace SGECAR.API.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public abstract class ApiControllerBase : ControllerBase
    {
        // CONVIERTE EL RESULTADO DE LA CAPA DE NEGOCIO EN UNA RESPUESTA HTTP
        protected IActionResult Resultado(OperacionResultado resultado)
        {
            var cuerpo = new MensajeResponse(resultado.Mensaje);

            if (resultado.Exitoso)
                return Ok(cuerpo);

            return resultado.TipoError switch
            {
                TipoError.NoEncontrado => NotFound(cuerpo),
                TipoError.Conflicto => Conflict(cuerpo),
                _ => BadRequest(cuerpo)
            };
        }

        protected static PermisoDto ToDto(Permiso permiso) => new()
        {
            PermisoId = permiso.PermisoId,
            Nombre = permiso.Nombre,
            Descripcion = permiso.Descripcion,
            EsSistema = Acciones.EsPermisoBase(permiso.Nombre)
        };

        protected static RolDto ToDto(Role rol) => new()
        {
            RolId = rol.RolId,
            Nombre = rol.Nombre,
            EsSistema = RolesSistema.EsRolProtegido(rol.Nombre),
            CantidadUsuarios = rol.Usuarios.Count,
            Permisos = rol.Permisos.OrderBy(p => p.PermisoId).Select(ToDto).ToList()
        };

        protected static UsuarioDto ToDto(Usuario usuario) => new()
        {
            UsuarioId = usuario.UsuarioId,
            Usuario = usuario.Usuario1,
            RolId = usuario.RolId,
            Rol = usuario.Rol?.Nombre ?? string.Empty,
            EstadoCuenta = usuario.EstadoCuenta,
            IntentosFallidos = usuario.IntentosFallidos
        };

        protected static ClienteDto ToDto(Cliente cliente) => new()
        {
            ClienteId = cliente.ClienteId,
            Nombre = cliente.Nombre,
            Apellido = cliente.Apellido,
            Telefono = cliente.Telefono,
            Correo = cliente.Correo,
            Direccion = cliente.Direccion
        };

        protected static CategoriaDto ToDto(Categoria categoria) => new()
        {
            CategoriaId = categoria.CategoriaId,
            Nombre = categoria.Nombre
        };

        protected static ProductoDto ToDto(Producto producto) => new()
        {
            ProductoId = producto.ProductoId,
            Codigo = producto.Codigo,
            Nombre = producto.Nombre,
            Precio = producto.Precio,
            Stock = producto.Stock,
            CategoriaId = producto.CategoriaId,
            Categoria = producto.Categoria?.Nombre ?? string.Empty
        };
    }
}
