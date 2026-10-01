using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SGECAR.App.Seguridad;
using SGECAR.App.Servicios;
using SGECAR.App.Utilidades;
using SGECAR.Shared.Contracts;
using SGECAR.Shared.Security;

namespace SGECAR.App.Pages.Usuarios;

public class CrearModel : PaginaProtegida
{
    private readonly ApiCliente _api;

    public CrearModel(ApiCliente api)
    {
        _api = api;
    }

    protected override string? AccionRequerida => Acciones.CrearUsuarios;

    public class DatosUsuario
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Debe tener entre 3 y 50 caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9._]+$", ErrorMessage = "Solo letras, números, punto o guion bajo.")]
        [Display(Name = "Nombre de usuario")]
        public string Usuario { get; set; } = "";

        // Mismas reglas que valida la API: mínimo 8 caracteres, con letras y números
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Debe tener al menos 8 caracteres.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Debe incluir letras y números.")]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; } = "";

        [Required(ErrorMessage = "Confirme la contraseña.")]
        [Compare(nameof(Contrasena), ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmarContrasena { get; set; } = "";

        [Required(ErrorMessage = "Seleccione un rol.")]
        [Display(Name = "Rol")]
        public int? RolId { get; set; }
    }

    [BindProperty]
    public DatosUsuario Entrada { get; set; } = new();

    public List<SelectListItem> OpcionesRol { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!Puede(Acciones.Agregar)) return PermisoDenegado(Acciones.Agregar);
        return await CargarRolesAsync() ?? Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!Puede(Acciones.Agregar)) return PermisoDenegado(Acciones.Agregar);

        if (!ModelState.IsValid)
            return await CargarRolesAsync() ?? Page();

        var respuesta = await _api.CrearUsuarioAsync(new UsuarioCrearRequest
        {
            Usuario = Entrada.Usuario.Trim(),
            Contrasena = Entrada.Contrasena,
            RolId = Entrada.RolId!.Value
        });

        var error = ManejarErrorApi(respuesta);
        if (error is not null) return error;

        if (!respuesta.Exitoso)
        {
            // 400 (datos inválidos) o 409 (usuario repetido): se muestra el mensaje de la API
            ModelState.AddModelError(string.Empty, respuesta.Mensaje);
            this.MostrarMessageBox(TipoMensaje.Error, "No se pudo crear", respuesta.Mensaje);
            return await CargarRolesAsync() ?? Page();
        }

        this.MostrarMessageBox(TipoMensaje.Exito, "Usuario creado",
            $"El usuario \"{respuesta.Datos?.Usuario ?? Entrada.Usuario}\" se creó correctamente.");
        return RedirectToPage("Index");
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
