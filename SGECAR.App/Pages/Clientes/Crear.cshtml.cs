using Microsoft.AspNetCore.Mvc;
using SGECAR.App.Seguridad;
using SGECAR.App.Servicios;
using SGECAR.App.Utilidades;
using SGECAR.Shared.Security;

namespace SGECAR.App.Pages.Clientes;

public class CrearModel : PaginaProtegida
{
    private readonly ApiCliente _api;

    public CrearModel(ApiCliente api)
    {
        _api = api;
    }

    protected override string? AccionRequerida => Acciones.Agregar;

    [BindProperty]
    public DatosCliente Entrada { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var respuesta = await _api.CrearClienteAsync(Entrada.ARequest());
        var error = ManejarErrorApi(respuesta);
        if (error is not null) return error;

        if (!respuesta.Exitoso)
        {
            
            ModelState.AddModelError(string.Empty, respuesta.Mensaje);
            this.MostrarMessageBox(TipoMensaje.Error, "No se pudo registrar", respuesta.Mensaje);
            return Page();
        }

        this.MostrarMessageBox(TipoMensaje.Exito, "Cliente registrado",
            $"El cliente \"{Entrada.Nombre.Trim()} {Entrada.Apellido.Trim()}\" se registró correctamente.");
        return RedirectToPage("Index");
    }
}
