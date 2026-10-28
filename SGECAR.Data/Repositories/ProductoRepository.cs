using Microsoft.EntityFrameworkCore;
using SGECAR.Data.Context;
using SGECAR.Shared.Contracts.Repositories;
using SGECAR.Shared.Models;

namespace SGECAR.Data.Repositories;

public class ProductoRepository : IProductoRepository
{
    private readonly GestionEmpresarialContext _context;

    public ProductoRepository(GestionEmpresarialContext context)
    {
        _context = context;
    }

    // LISTAR PRODUCTOS CON SU CATEGORÍA (buscar FILTRA POR CÓDIGO O NOMBRE)
    public async Task<List<Producto>> ListProducts(string? buscar = null, int? categoriaId = null)
    {
        var consulta = _context.Productos
            .Include(p => p.Categoria)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();

            // SIN DISTINGUIR ACENTOS: "inalambrico" ENCUENTRA "inalámbrico"
            consulta = consulta.Where(p =>
                p.Codigo.Contains(buscar) ||
                EF.Functions.Collate(p.Nombre, Busqueda.Intercalacion).Contains(buscar));
        }

        if (categoriaId.HasValue)
            consulta = consulta.Where(p => p.CategoriaId == categoriaId.Value);

        return await consulta
            .OrderBy(p => p.Codigo)
            .ToListAsync();
    }

    // OBTENER PRODUCTO POR ID (INCLUYE CATEGORÍA)
    public async Task<Producto?> GetProductById(int productoId)
    {
        return await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.ProductoId == productoId);
    }

    // VERIFICAR SI YA EXISTE UN CÓDIGO
    public async Task<bool> ExistsCode(string codigo, int? excluirProductoId = null)
    {
        return await _context.Productos
            .AnyAsync(p => p.Codigo == codigo && p.ProductoId != excluirProductoId);
    }

    // CREAR PRODUCTO
    public async Task CreateProduct(Producto producto)
    {
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
    }

    // ELIMINAR PRODUCTO
    public async Task DeleteProduct(Producto producto)
    {
        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();
    }

    // LISTAR CATEGORÍAS
    public async Task<List<Categoria>> ListCategories()
    {
        return await _context.Categorias
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    // VERIFICAR SI EXISTE UNA CATEGORÍA
    public async Task<bool> ExistsCategory(int categoriaId)
    {
        return await _context.Categorias
            .AnyAsync(c => c.CategoriaId == categoriaId);
    }

    // GUARDAR CAMBIOS DE ENTIDADES YA RASTREADAS
    public async Task SaveChanges()
    {
        await _context.SaveChangesAsync();
    }
}
