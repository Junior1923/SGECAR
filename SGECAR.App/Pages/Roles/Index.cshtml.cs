using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SGECAR.App.Seguridad;
using SGECAR.App.Servicios;
using SGECAR.App.Utilidades;
using SGECAR.Shared.Contracts;
using SGECAR.Shared.Security;

namespace SGECAR.App.Pages.Roles;

public class IndexModel : PaginaProtegida
{
    private readonly ApiCliente _api;

    public IndexModel(ApiCliente api)
    {
        _api = api;
    }

    // Solo entra quien puede crear roles (Administrador)
    protected override string? AccionRequerida => Acciones.CrearRoles;

    public List<RolDto> Roles { get; private set; } = new();
    public List<PermisoDto> Permisos { get; private set; } = new();

    [BindProperty]
    [Required(ErrorMessage = "Escriba el nombre del rol.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Debe tener entre 3 y 50 caracteres.")]
    [Display(Name = "Nombre del rol")]
    public string NombreRol { get; set; } = "";

    [BindProperty]
    public List<int> PermisoIds { get; set; } = new();

    public async Task<IActionResult> OnGetAsync() => await CargarAsync() ?? Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!Puede(Acciones.Agregar)) return PermisoDenegado(Acciones.Agregar);

        if (PermisoIds.Count == 0)
            ModelState.AddModelError(nameof(PermisoIds), "Marque al menos un permiso.");

        if (!ModelState.IsValid)
            return await CargarAsync() ?? Page();

        var respuesta = await _api.CrearRolAsync(new RolRequest { Nombre = NombreRol.Trim(), PermisoIds = PermisoIds });
        var error = ManejarErrorApi(respuesta);
        if (error is not null) return error;

        if (!respuesta.Exitoso)
        {
            // 409 si ya existe un rol con ese nombre
            ModelState.AddModelError(string.Empty, respuesta.Mensaje);
            this.MostrarMessageBox(TipoMensaje.Error, "No se pudo crear", respuesta.Mensaje);
            return await CargarAsync() ?? Page();
        }

        this.MostrarMessageBox(TipoMensaje.Exito, "Rol creado",
            $"El rol \"{respuesta.Datos?.Nombre ?? NombreRol}\" se creó con {PermisoIds.Count} permiso(s).");
        return RedirectToPage();
    }

    private async Task<IActionResult?> CargarAsync()
    {
        var roles = await _api.ListarRolesAsync();
        var error = ManejarErrorApi(roles);
        if (error is not null) return error;

        var permisos = await _api.ListarPermisosAsync();
        error = ManejarErrorApi(permisos);
        if (error is not null) return error;

        Roles = roles.Datos ?? new();
        Permisos = (permisos.Datos ?? new()).OrderBy(p => p.PermisoId).ToList();
        return null;
    }
}
