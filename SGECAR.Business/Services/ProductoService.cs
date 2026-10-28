using SGECAR.Shared.Contracts;
using SGECAR.Shared.Contracts.Repositories;
using SGECAR.Shared.Contracts.Services;
using SGECAR.Shared.Models;

namespace SGECAR.Business.Services
{
    public class ProductoService : IProductoService
    {
        private const int LongitudMaximaCodigo = 50;
        private const int LongitudMaximaNombre = 150;

        // MÁXIMO QUE CABE EN LA COLUMNA DECIMAL(18,2)
        private const decimal PrecioMaximo = 9_999_999_999_999_999.99m;

        private readonly IProductoRepository _productos;

        public ProductoService(IProductoRepository productos)
        {
            _productos = productos;
        }

        public Task<List<Producto>> ListProductsAsync(string? buscar = null, int? categoriaId = null) =>
            _productos.ListProducts(buscar, categoriaId);

        public Task<Producto?> GetProductAsync(int productoId) => _productos.GetProductById(productoId);

        public Task<List<Categoria>> ListCategoriesAsync() => _productos.ListCategories();

        public async Task<OperacionResultado> CreateProductAsync(ProductoRequest datos)
        {
            var error = await Validar(datos, null);
            if (error != null)
                return error;

            var producto = new Producto();
            AsignarDatos(producto, datos);

            await _productos.CreateProduct(producto);

            return OperacionResultado.Ok($"Producto \"{producto.Codigo}\" creado correctamente.", producto.ProductoId);
        }

        public async Task<OperacionResultado> UpdateProductAsync(int productoId, ProductoRequest datos)
        {
            var producto = await _productos.GetProductById(productoId);

            if (producto == null)
                return OperacionResultado.NoEncontrado("El producto no existe.");

            var error = await Validar(datos, productoId);
            if (error != null)
                return error;

            AsignarDatos(producto, datos);

            await _productos.SaveChanges();

            return OperacionResultado.Ok($"Producto \"{producto.Codigo}\" actualizado correctamente.");
        }

        public async Task<OperacionResultado> DeleteProductAsync(int productoId)
        {
            var producto = await _productos.GetProductById(productoId);

            if (producto == null)
                return OperacionResultado.NoEncontrado("El producto no existe.");

            await _productos.DeleteProduct(producto);

            return OperacionResultado.Ok($"Producto \"{producto.Codigo}\" eliminado correctamente.");
        }

        private async Task<OperacionResultado?> Validar(ProductoRequest datos, int? productoId)
        {
            if (string.IsNullOrWhiteSpace(datos.Codigo))
                return OperacionResultado.Error("El código es obligatorio.");

            var codigo = datos.Codigo.Trim();

            if (codigo.Length > LongitudMaximaCodigo)
                return OperacionResultado.Error($"El código no puede exceder {LongitudMaximaCodigo} caracteres.");

            if (await _productos.ExistsCode(codigo, productoId))
                return OperacionResultado.Conflicto($"Ya existe un producto con el código \"{codigo}\".");

            if (string.IsNullOrWhiteSpace(datos.Nombre))
                return OperacionResultado.Error("El nombre es obligatorio.");

            if (datos.Nombre.Trim().Length > LongitudMaximaNombre)
                return OperacionResultado.Error($"El nombre no puede exceder {LongitudMaximaNombre} caracteres.");

            if (datos.Precio < 0)
                return OperacionResultado.Error("El precio no puede ser negativo.");

            if (datos.Precio > PrecioMaximo)
                return OperacionResultado.Error("El precio excede el máximo permitido.");

            if (decimal.Round(datos.Precio, 2) != datos.Precio)
                return OperacionResultado.Error("El precio no puede tener más de 2 decimales.");

            if (datos.Stock < 0)
                return OperacionResultado.Error("El stock no puede ser negativo.");

            if (!await _productos.ExistsCategory(datos.CategoriaId))
                return OperacionResultado.Error("Seleccione una categoría válida.");

            return null;
        }

        private static void AsignarDatos(Producto producto, ProductoRequest datos)
        {
            producto.Codigo = datos.Codigo.Trim();
            producto.Nombre = datos.Nombre.Trim();
            producto.Precio = datos.Precio;
            producto.Stock = datos.Stock;
            producto.CategoriaId = datos.CategoriaId;
        }
    }
}
