using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SGECAR.App.Seguridad;
using SGECAR.App.Servicios;
using SGECAR.App.Utilidades;
using SGECAR.Shared.Security;

namespace SGECAR.App.Pages.Productos;

public class EditarModel : PaginaProtegida
{
    private readonly ApiCliente _api;

    public EditarModel(ApiCliente api)
    {
        _api = api;
    }

    protected override string? AccionRequerida => Acciones.Modificar;

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public DatosProducto Entrada { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var producto = await _api.ObtenerProductoAsync(Id);
        var error = ManejarErrorApi(producto);
        if (error is not null) return error;

        if (!producto.Exitoso || producto.Datos is null)
        {
            this.MostrarMessageBox(TipoMensaje.Error, "No encontrado", producto.Mensaje);
            return RedirectToPage("Index");
        }

        Entrada = DatosProducto.Desde(producto.Datos);
        return await CargarCategoriasAsync() ?? Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return await CargarCategoriasAsync() ?? Page();

        var respuesta = await _api.ModificarProductoAsync(Id, Entrada.ARequest());
        var error = ManejarErrorApi(respuesta);
        if (error is not null) return error;

        if (!respuesta.Exitoso)
        {
            ModelState.AddModelError(string.Empty, respuesta.Mensaje);
            this.MostrarMessageBox(TipoMensaje.Error, "No se pudo modificar", respuesta.Mensaje);
            return await CargarCategoriasAsync() ?? Page();
        }

        this.MostrarMessageBox(TipoMensaje.Exito, "Cambios guardados", respuesta.Datos?.Mensaje ?? "");
        return RedirectToPage("Index");
    }

    private async Task<IActionResult?> CargarCategoriasAsync()
    {
        var categorias = await _api.ListarCategoriasAsync();
        var error = ManejarErrorApi(categorias);
        if (error is not null) return error;

        ViewData["Categorias"] = (categorias.Datos ?? new())
            .Select(c => new SelectListItem(c.Nombre, c.CategoriaId.ToString()))
            .ToList();
        return null;
    }
}
