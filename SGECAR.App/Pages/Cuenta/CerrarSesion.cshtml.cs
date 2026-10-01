using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SGECAR.App.Servicios;
using SGECAR.App.Utilidades;

namespace SGECAR.App.Pages.Cuenta;

public class CerrarSesionModel : PageModel
{
    private readonly ApiCliente _api;

    public CerrarSesionModel(ApiCliente api)
    {
        _api = api;
    }

    public IActionResult OnGet() => RedirectToPage("/Cuenta/Login");

    public async Task<IActionResult> OnPostAsync()
    {
        // Invalida el token en la API y limpia la sesión de la App
        await _api.LogoutAsync();
        this.MostrarMessageBox(TipoMensaje.Informacion, "Sesión cerrada", "Ha cerrado sesión correctamente.");
        return RedirectToPage("/Cuenta/Login");
    }
}
