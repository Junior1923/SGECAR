using SGECAR.Shared.Models;

namespace SGECAR.Shared.Contracts.Repositories;

public interface IProductoRepository
{
    // LISTAR PRODUCTOS CON SU CATEGORÍA (buscar FILTRA POR CÓDIGO O NOMBRE)
    Task<List<Producto>> ListProducts(string? buscar = null, int? categoriaId = null);

    // OBTENER PRODUCTO POR ID (INCLUYE CATEGORÍA)
    Task<Producto?> GetProductById(int productoId);

    // VERIFICAR SI YA EXISTE UN CÓDIGO
    Task<bool> ExistsCode(string codigo, int? excluirProductoId = null);

    // CREAR PRODUCTO
    Task CreateProduct(Producto producto);

    // ELIMINAR PRODUCTO
    Task DeleteProduct(Producto producto);

    // LISTAR CATEGORÍAS
    Task<List<Categoria>> ListCategories();

    // VERIFICAR SI EXISTE UNA CATEGORÍA
    Task<bool> ExistsCategory(int categoriaId);

    // GUARDAR CAMBIOS DE ENTIDADES YA RASTREADAS
    Task SaveChanges();
}
