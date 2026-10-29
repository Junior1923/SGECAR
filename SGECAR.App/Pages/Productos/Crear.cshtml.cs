using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SGECAR.App.Seguridad;
using SGECAR.App.Servicios;
using SGECAR.App.Utilidades;
using SGECAR.Shared.Security;

namespace SGECAR.App.Pages.Productos;

public class CrearModel : PaginaProtegida
{
    private readonly ApiCliente _api;

    public CrearModel(ApiCliente api)
    {
        _api = api;
    }

    protected override string? AccionRequerida => Acciones.Agregar;

    [BindProperty]
    public DatosProducto Entrada { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        return await CargarCategoriasAsync() ?? Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return await CargarCategoriasAsync() ?? Page();

        var respuesta = await _api.CrearProductoAsync(Entrada.ARequest());
        var error = ManejarErrorApi(respuesta);
        if (error is not null) return error;

        if (!respuesta.Exitoso)
        {
            // 400 (datos inválidos) o 409 (código repetido)
            ModelState.AddModelError(string.Empty, respuesta.Mensaje);
            this.MostrarMessageBox(TipoMensaje.Error, "No se pudo registrar", respuesta.Mensaje);
            return await CargarCategoriasAsync() ?? Page();
        }

        this.MostrarMessageBox(TipoMensaje.Exito, "Producto registrado",
            $"El producto \"{respuesta.Datos?.Codigo ?? Entrada.Codigo} - {Entrada.Nombre.Trim()}\" se registró correctamente.");
        return RedirectToPage("Index");
    }

    // Llena el ComboBox de categorías desde la API
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
