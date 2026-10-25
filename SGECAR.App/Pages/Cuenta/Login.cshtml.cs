using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SGECAR.App.Servicios;
using SGECAR.App.Utilidades;

namespace SGECAR.App.Pages.Cuenta;

public class LoginModel : PageModel
{
    private readonly ApiCliente _api;

    public LoginModel(ApiCliente api)
    {
        _api = api;
    }

    [BindProperty]
    [Required(ErrorMessage = "Debe escribir su usuario.")]
    [StringLength(50, ErrorMessage = "El usuario no puede pasar de 50 caracteres.")]
    public string Usuario { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "Debe escribir su contraseña.")]
    [DataType(DataType.Password)]
    public string Contrasena { get; set; } = "";

    public IActionResult OnGet()
    {
        if (HttpContext.Session.ObtenerUsuario() is not null)
            return RedirectToPage("/Menu/Index");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            this.MostrarMessageBox(TipoMensaje.Advertencia, "Campos vacíos",
                "Debe completar el usuario y la contraseña para continuar.");
            return Page();
        }

        // La API valida credenciales, cuenta los intentos y bloquea la cuenta
        var respuesta = await _api.LoginAsync(Usuario.Trim(), Contrasena);

        // La contraseña no se devuelve a la vista
        Contrasena = "";
        ModelState.Remove(nameof(Contrasena));

        if (respuesta.Exitoso && respuesta.Datos is not null)
        {
            var sesion = respuesta.Datos.Sesion;
            this.MostrarMessageBox(TipoMensaje.Exito, respuesta.Mensaje,
                $"Ha iniciado sesión con el rol {sesion.Rol}.");
            return RedirectToPage("/Menu/Index");
        }

        if (respuesta.CuentaBloqueada)
            this.MostrarMessageBox(TipoMensaje.Error, "Cuenta bloqueada", respuesta.Mensaje);
        else if (respuesta.Codigo == 0)
            this.MostrarMessageBox(TipoMensaje.Error, "Sin conexión", respuesta.Mensaje);
        else
            this.MostrarMessageBox(TipoMensaje.Error, "Acceso denegado", respuesta.Mensaje);

        return Page();
    }
}
