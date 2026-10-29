using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SGECAR.App.Seguridad;
using SGECAR.App.Servicios;
using SGECAR.Shared.Contracts;
using SGECAR.Shared.Security;

namespace SGECAR.App.Pages.Reportes;

public class UsuariosModel : PaginaProtegida
{
    private const int MaximoIntentos = 3;
    private readonly ApiCliente _api;

    public UsuariosModel(ApiCliente api)
    {
        _api = api;
    }

    // El reporte usa GET /api/usuarios, que exige este permiso
    protected override string? AccionRequerida => Acciones.CrearUsuarios;

    // Filtros del formulario
    [BindProperty(SupportsGet = true)] public bool Generar { get; set; }
    [BindProperty(SupportsGet = true)] public int? Id { get; set; }
    [BindProperty(SupportsGet = true)] public string? Buscar { get; set; }
    [BindProperty(SupportsGet = true)] public int? RolId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Estado { get; set; }

    public bool Generado { get; private set; }
    public DateTime FechaGeneracion { get; private set; }
    public List<UsuarioDto> Usuarios { get; private set; } = new();
    public List<SelectListItem> OpcionesRol { get; private set; } = new();
    public List<string> FiltrosAplicados { get; private set; } = new();
    public Dictionary<string, int> PorRol { get; private set; } = new();

    public static string EstadoDe(UsuarioDto u) =>
        u.EstadoCuenta ? "Activo" : u.IntentosFallidos >= MaximoIntentos ? "Bloqueado" : "Inactivo";

    public async Task<IActionResult> OnGetAsync()
    {
        var roles = await _api.ListarRolesAsync();
        var error = ManejarErrorApi(roles);
        if (error is not null) return error;

        var listaRoles = roles.Datos ?? new();
        OpcionesRol = listaRoles
            .Select(r => new SelectListItem(r.Nombre, r.RolId.ToString(), r.RolId == RolId))
            .ToList();

        // El reporte solo se genera cuando se envía el formulario
        if (!Generar) return Page();

        var respuesta = await _api.ListarUsuariosAsync();
        error = ManejarErrorApi(respuesta);
        if (error is not null) return error;

        // Filtros dinámicos: solo se aplican los que el usuario llenó
        IEnumerable<UsuarioDto> consulta = respuesta.Datos ?? new();
        if (Id.HasValue)
        {
            consulta = consulta.Where(u => u.UsuarioId == Id.Value);
            FiltrosAplicados.Add($"ID = {Id}");
        }
        if (!string.IsNullOrWhiteSpace(Buscar))
        {
            consulta = consulta.Where(u => u.Usuario.Contains(Buscar.Trim(), StringComparison.OrdinalIgnoreCase));
            FiltrosAplicados.Add($"Nombre contiene \"{Buscar.Trim()}\"");
        }
        if (RolId.HasValue)
        {
            consulta = consulta.Where(u => u.RolId == RolId.Value);
            FiltrosAplicados.Add($"Rol = {listaRoles.FirstOrDefault(r => r.RolId == RolId)?.Nombre}");
        }
        if (!string.IsNullOrWhiteSpace(Estado))
        {
            consulta = consulta.Where(u => EstadoDe(u) == Estado);
            FiltrosAplicados.Add($"Estado = {Estado}");
        }

        Usuarios = consulta.OrderBy(u => u.UsuarioId).ToList();
        PorRol = Usuarios.GroupBy(u => u.Rol).OrderBy(g => g.Key).ToDictionary(g => g.Key, g => g.Count());
        FechaGeneracion = DateTime.Now;
        Generado = true;
        return Page();
    }
}
