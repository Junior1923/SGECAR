using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SGECAR.App.Seguridad;
using SGECAR.App.Servicios;
using SGECAR.App.Utilidades;
using SGECAR.Shared.Contracts;
using SGECAR.Shared.Security;

namespace SGECAR.App.Pages.Clientes;

public class IndexModel : PaginaProtegida
{
    private readonly ApiCliente _api;

    public IndexModel(ApiCliente api)
    {
        _api = api;
    }

    // Los tres roles tienen CONSULTAR; agregar, modificar y eliminar se revisan en cada acción
    protected override string? AccionRequerida => Acciones.Consultar;

    [BindProperty(SupportsGet = true)] public int? Id { get; set; }
    [BindProperty(SupportsGet = true)] public string? Buscar { get; set; }
    [BindProperty(SupportsGet = true)] public string? Ubicacion { get; set; }
    [BindProperty(SupportsGet = true)] public string? Contacto { get; set; }

    public List<ClienteDto> Clientes { get; private set; } = new();
    public List<SelectListItem> OpcionesUbicacion { get; private set; } = new();
    public int Total { get; private set; }
    public int ConTelefono { get; private set; }
    public int SinTelefono { get; private set; }

    public bool HayFiltros => Id.HasValue || !string.IsNullOrWhiteSpace(Buscar)
        || !string.IsNullOrWhiteSpace(Ubicacion) || !string.IsNullOrWhiteSpace(Contacto);

    public async Task<IActionResult> OnGetAsync()
    {
        // Lista completa: para los indicadores y la lista de ubicaciones
        var todos = await _api.ListarClientesAsync();
        var error = ManejarErrorApi(todos);
        if (error is not null) return error;

        var lista = todos.Datos ?? new();
        Total = lista.Count;
        ConTelefono = lista.Count(c => !string.IsNullOrWhiteSpace(c.Telefono));
        SinTelefono = Total - ConTelefono;

        OpcionesUbicacion = lista
            .Where(c => !string.IsNullOrWhiteSpace(c.Direccion))
            .Select(c => c.Direccion!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(d => d)
            .Select(d => new SelectListItem(d, d, string.Equals(d, Ubicacion, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        // Búsqueda por nombre: la hace la API (nombre, apellido, teléfono o correo)
        IEnumerable<ClienteDto> consulta = lista;
        if (!string.IsNullOrWhiteSpace(Buscar))
        {
            var filtrados = await _api.ListarClientesAsync(Buscar);
            error = ManejarErrorApi(filtrados);
            if (error is not null) return error;
            consulta = filtrados.Datos ?? new();
        }

        // Filtros dinámicos sobre el resultado
        if (Id.HasValue)
            consulta = consulta.Where(c => c.ClienteId == Id.Value);
        if (!string.IsNullOrWhiteSpace(Ubicacion))
            consulta = consulta.Where(c => string.Equals(c.Direccion?.Trim(), Ubicacion, StringComparison.OrdinalIgnoreCase));
        if (Contacto == "con-telefono")
            consulta = consulta.Where(c => !string.IsNullOrWhiteSpace(c.Telefono));
        else if (Contacto == "sin-telefono")
            consulta = consulta.Where(c => string.IsNullOrWhiteSpace(c.Telefono));

        Clientes = consulta.OrderBy(c => c.ClienteId).ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        if (!Puede(Acciones.Eliminar)) return PermisoDenegado(Acciones.Eliminar);

        var respuesta = await _api.EliminarClienteAsync(id);
        var error = ManejarErrorApi(respuesta);
        if (error is not null) return error;

        if (respuesta.Exitoso)
            this.MostrarMessageBox(TipoMensaje.Exito, "Cliente eliminado", respuesta.Datos?.Mensaje ?? "");
        else
            this.MostrarMessageBox(TipoMensaje.Error, "No se pudo eliminar", respuesta.Mensaje);

        return RedirectToPage();
    }
}
