using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SGECAR.App.Servicios;

namespace SGECAR.App.Pages;

// Página raíz: envía al menú si hay sesión, o al login si no
public class IndexModel : PageModel
{
    public IActionResult OnGet() =>
        HttpContext.Session.ObtenerUsuario() is null
            ? RedirectToPage("/Cuenta/Login")
            : RedirectToPage("/Menu/Index");
}
