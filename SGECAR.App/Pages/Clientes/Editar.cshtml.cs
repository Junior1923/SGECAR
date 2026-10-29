using Microsoft.AspNetCore.Mvc;
using SGECAR.App.Seguridad;
using SGECAR.App.Servicios;
using SGECAR.App.Utilidades;
using SGECAR.Shared.Security;

namespace SGECAR.App.Pages.Clientes;

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
    public DatosCliente Entrada { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var cliente = await _api.ObtenerClienteAsync(Id);
        var error = ManejarErrorApi(cliente);
        if (error is not null) return error;

        if (!cliente.Exitoso || cliente.Datos is null)
        {
            this.MostrarMessageBox(TipoMensaje.Error, "No encontrado", cliente.Mensaje);
            return RedirectToPage("Index");
        }

        Entrada = DatosCliente.Desde(cliente.Datos);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var respuesta = await _api.ModificarClienteAsync(Id, Entrada.ARequest());
        var error = ManejarErrorApi(respuesta);
        if (error is not null) return error;

        if (!respuesta.Exitoso)
        {
            ModelState.AddModelError(string.Empty, respuesta.Mensaje);
            this.MostrarMessageBox(TipoMensaje.Error, "No se pudo modificar", respuesta.Mensaje);
            return Page();
        }

        this.MostrarMessageBox(TipoMensaje.Exito, "Cambios guardados", respuesta.Datos?.Mensaje ?? "");
        return RedirectToPage("Index");
    }
}
