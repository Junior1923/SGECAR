using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SGECAR.App.Seguridad;
using SGECAR.App.Servicios;
using SGECAR.App.Utilidades;
using SGECAR.Shared.Contracts;
using SGECAR.Shared.Security;

namespace SGECAR.App.Pages.Usuarios;

public class EditarModel : PaginaProtegida
{
    private readonly ApiCliente _api;

    public EditarModel(ApiCliente api)
    {
        _api = api;
    }

    protected override string? AccionRequerida => Acciones.CrearUsuarios;

    public class DatosEdicion
    {
        [Required(ErrorMessage = "Seleccione un rol.")]
        [Display(Name = "Rol")]
        public int? RolId { get; set; }

        public bool EstadoCuenta { get; set; }

        [StringLength(100, MinimumLength = 8, ErrorMessage = "Debe tener al menos 8 caracteres.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Debe incluir letras y números.")]
        [Display(Name = "Nueva contraseña (opcional)")]
        public string? NuevaContrasena { get; set; }
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public DatosEdicion Entrada { get; set; } = new();

    public string NombreUsuario { get; private set; } = "";
    public List<SelectListItem> OpcionesRol { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!Puede(Acciones.Modificar)) return PermisoDenegado(Acciones.Modificar);

        var usuario = await _api.ObtenerUsuarioAsync(Id);
        var error = ManejarErrorApi(usuario);
        if (error is not null) return error;

        if (!usuario.Exitoso || usuario.Datos is null)
        {
            this.MostrarMessageBox(TipoMensaje.Error, "No encontrado", usuario.Mensaje);
            return RedirectToPage("Index");
        }

        NombreUsuario = usuario.Datos.Usuario;
        Entrada = new DatosEdicion { RolId = usuario.Datos.RolId, EstadoCuenta = usuario.Datos.EstadoCuenta };
        return await CargarRolesAsync() ?? Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!Puede(Acciones.Modificar)) return PermisoDenegado(Acciones.Modificar);

        if (!ModelState.IsValid)
            return await RecargarAsync();

        var respuesta = await _api.ModificarUsuarioAsync(Id, new UsuarioActualizarRequest
        {
            RolId = Entrada.RolId!.Value,
            EstadoCuenta = Entrada.EstadoCuenta,
            NuevaContrasena = string.IsNullOrWhiteSpace(Entrada.NuevaContrasena) ? null : Entrada.NuevaContrasena
        });

        var error = ManejarErrorApi(respuesta);
        if (error is not null) return error;

        if (!respuesta.Exitoso)
        {
            // Por ejemplo: un usuario no puede cambiar su propio rol ni desactivarse
            ModelState.AddModelError(string.Empty, respuesta.Mensaje);
            return await RecargarAsync();
        }

        this.MostrarMessageBox(TipoMensaje.Exito, "Cambios guardados", respuesta.Datos?.Mensaje ?? "");
        return RedirectToPage("Index");
    }

    private async Task<IActionResult> RecargarAsync()
    {
        var usuario = await _api.ObtenerUsuarioAsync(Id);
        NombreUsuario = usuario.Datos?.Usuario ?? "";
        return await CargarRolesAsync() ?? Page();
    }

    private async Task<IActionResult?> CargarRolesAsync()
    {
        var roles = await _api.ListarRolesAsync();
        var error = ManejarErrorApi(roles);
        if (error is not null) return error;

        OpcionesRol = (roles.Datos ?? new())
            .Select(r => new SelectListItem(r.Nombre, r.RolId.ToString()))
            .ToList();
        return null;
    }
}
