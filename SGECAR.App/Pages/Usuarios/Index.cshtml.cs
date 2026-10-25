using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SGECAR.App.Seguridad;
using SGECAR.App.Servicios;
using SGECAR.App.Utilidades;
using SGECAR.Shared.Contracts;
using SGECAR.Shared.Security;

namespace SGECAR.App.Pages.Usuarios;

public class IndexModel : PaginaProtegida
{
    private const int MaximoIntentos = 3;
    private readonly ApiCliente _api;

    public IndexModel(ApiCliente api)
    {
        _api = api;
    }

    // Solo entra quien puede gestionar usuarios (Administrador)
    protected override string? AccionRequerida => Acciones.CrearUsuarios;

    [BindProperty(SupportsGet = true)] public string? Buscar { get; set; }
    [BindProperty(SupportsGet = true)] public int? RolId { get; set; }

    public List<UsuarioDto> Usuarios { get; private set; } = new();
    public List<SelectListItem> OpcionesRol { get; private set; } = new();
    public int Total { get; private set; }
    public int Activos { get; private set; }
    public int Inactivos { get; private set; }
    public int Bloqueados { get; private set; }

    // La API marca la cuenta como inactiva al bloquearla; se distingue por los intentos
    public static bool EstaBloqueado(UsuarioDto u) => !u.EstadoCuenta && u.IntentosFallidos >= MaximoIntentos;

    public async Task<IActionResult> OnGetAsync()
    {
        var respuesta = await _api.ListarUsuariosAsync();
        var error = ManejarErrorApi(respuesta);
        if (error is not null) return error;

        var todos = respuesta.Datos ?? new();
        Total = todos.Count;
        Activos = todos.Count(u => u.EstadoCuenta);
        Bloqueados = todos.Count(EstaBloqueado);
        Inactivos = Total - Activos - Bloqueados;

        // Consulta dinámica según la búsqueda y el rol elegido
        IEnumerable<UsuarioDto> consulta = todos;
        if (!string.IsNullOrWhiteSpace(Buscar))
            consulta = consulta.Where(u => u.Usuario.Contains(Buscar.Trim(), StringComparison.OrdinalIgnoreCase));
        if (RolId.HasValue)
            consulta = consulta.Where(u => u.RolId == RolId.Value);
        Usuarios = consulta.OrderBy(u => u.UsuarioId).ToList();

        var roles = await _api.ListarRolesAsync();
        OpcionesRol = (roles.Datos ?? new())
            .Select(r => new SelectListItem(r.Nombre, r.RolId.ToString(), r.RolId == RolId))
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostDesbloquearAsync(int id)
    {
        if (!Puede(Acciones.Modificar)) return PermisoDenegado(Acciones.Modificar);

        var respuesta = await _api.DesbloquearUsuarioAsync(id);
        var error = ManejarErrorApi(respuesta);
        if (error is not null) return error;

        MostrarResultado(respuesta, "Cuenta desbloqueada", "No se pudo desbloquear");
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        if (!Puede(Acciones.Eliminar)) return PermisoDenegado(Acciones.Eliminar);

        var respuesta = await _api.EliminarUsuarioAsync(id);
        var error = ManejarErrorApi(respuesta);
        if (error is not null) return error;

        MostrarResultado(respuesta, "Usuario eliminado", "No se pudo eliminar");
        return RedirectToPage();
    }

    private void MostrarResultado(RespuestaApi<MensajeResponse> respuesta, string tituloExito, string tituloError)
    {
        if (respuesta.Exitoso)
            this.MostrarMessageBox(TipoMensaje.Exito, tituloExito, respuesta.Datos?.Mensaje ?? "");
        else
            this.MostrarMessageBox(TipoMensaje.Error, tituloError, respuesta.Mensaje);
    }
}
