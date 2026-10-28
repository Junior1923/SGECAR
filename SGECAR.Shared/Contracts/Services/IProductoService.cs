using SGECAR.Shared.Models;

namespace SGECAR.Shared.Contracts.Services;

public interface IProductoService
{
    Task<List<Producto>> ListProductsAsync(string? buscar = null, int? categoriaId = null);

    Task<Producto?> GetProductAsync(int productoId);

    Task<OperacionResultado> CreateProductAsync(ProductoRequest datos);

    Task<OperacionResultado> UpdateProductAsync(int productoId, ProductoRequest datos);

    Task<OperacionResultado> DeleteProductAsync(int productoId);

    // CATEGORÍAS PARA LA LISTA DESPLEGABLE DEL FORMULARIO
    Task<List<Categoria>> ListCategoriesAsync();
}
