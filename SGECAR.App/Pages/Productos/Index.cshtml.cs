using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SGECAR.App.Seguridad;
using SGECAR.App.Servicios;
using SGECAR.App.Utilidades;
using SGECAR.Shared.Contracts;
using SGECAR.Shared.Security;

namespace SGECAR.App.Pages.Productos;

public class IndexModel : PaginaProtegida
{
    // Con esta cantidad o menos, el producto se marca como "stock bajo"
    public const int StockBajo = 5;

    private readonly ApiCliente _api;

    public IndexModel(ApiCliente api)
    {
        _api = api;
    }

    protected override string? AccionRequerida => Acciones.Consultar;

    [BindProperty(SupportsGet = true)] public int? Id { get; set; }
    [BindProperty(SupportsGet = true)] public string? Buscar { get; set; }
    [BindProperty(SupportsGet = true)] public int? CategoriaId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Existencia { get; set; }

    public List<ProductoDto> Productos { get; private set; } = new();
    public List<SelectListItem> OpcionesCategoria { get; private set; } = new();
    public int Total { get; private set; }
    public int Unidades { get; private set; }
    public int Agotados { get; private set; }
    public decimal ValorInventario { get; private set; }

    public bool HayFiltros => Id.HasValue || !string.IsNullOrWhiteSpace(Buscar)
        || CategoriaId.HasValue || !string.IsNullOrWhiteSpace(Existencia);

    public static string Moneda(decimal valor) => "RD$ " + valor.ToString("N2", CultureInfo.InvariantCulture);

    public async Task<IActionResult> OnGetAsync()
    {
        var todos = await _api.ListarProductosAsync();
        var error = ManejarErrorApi(todos);
        if (error is not null) return error;

        var lista = todos.Datos ?? new();
        Total = lista.Count;
        Unidades = lista.Sum(p => p.Stock);
        Agotados = lista.Count(p => p.Stock == 0);
        ValorInventario = lista.Sum(p => p.Precio * p.Stock);

        var categorias = await _api.ListarCategoriasAsync();
        error = ManejarErrorApi(categorias);
        if (error is not null) return error;

        OpcionesCategoria = (categorias.Datos ?? new())
            .Select(c => new SelectListItem(c.Nombre, c.CategoriaId.ToString(), c.CategoriaId == CategoriaId))
            .ToList();

        // Código/nombre y categoría: los filtra la API
        IEnumerable<ProductoDto> consulta = lista;
        if (!string.IsNullOrWhiteSpace(Buscar) || CategoriaId.HasValue)
        {
            var filtrados = await _api.ListarProductosAsync(Buscar, CategoriaId);
            error = ManejarErrorApi(filtrados);
            if (error is not null) return error;
            consulta = filtrados.Datos ?? new();
        }

        // ID y existencia: se filtran aquí
        if (Id.HasValue)
            consulta = consulta.Where(p => p.ProductoId == Id.Value);
        consulta = Existencia switch
        {
            "disponible" => consulta.Where(p => p.Stock > StockBajo),
            "bajo" => consulta.Where(p => p.Stock > 0 && p.Stock <= StockBajo),
            "agotado" => consulta.Where(p => p.Stock == 0),
            _ => consulta
        };

        Productos = consulta.OrderBy(p => p.ProductoId).ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        if (!Puede(Acciones.Eliminar)) return PermisoDenegado(Acciones.Eliminar);

        var respuesta = await _api.EliminarProductoAsync(id);
        var error = ManejarErrorApi(respuesta);
        if (error is not null) return error;

        if (respuesta.Exitoso)
            this.MostrarMessageBox(TipoMensaje.Exito, "Producto eliminado", respuesta.Datos?.Mensaje ?? "");
        else
            this.MostrarMessageBox(TipoMensaje.Error, "No se pudo eliminar", respuesta.Mensaje);

        return RedirectToPage();
    }
}
