using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SGECAR.App.Seguridad;
using SGECAR.App.Servicios;
using SGECAR.App.Utilidades;
using SGECAR.Shared.Contracts;
using SGECAR.Shared.Security;

namespace SGECAR.App.Pages.Dashboard;

// Panel de resumen. Cada sección se muestra según los permisos del rol,
// igual que el menú: la parte de usuarios solo la ve quien puede gestionarlos.
public class IndexModel : PaginaProtegida
{
    public const int StockBajo = 5;
    private readonly ApiCliente _api;

    public IndexModel(ApiCliente api)
    {
        _api = api;
    }

    // Los tres roles tienen CONSULTAR
    protected override string? AccionRequerida => Acciones.Consultar;

    public record Categoria(string Nombre, int Productos, int Unidades, decimal Valor);
    public record Permiso(string Nombre, bool Tiene);

    public bool VeUsuarios { get; private set; }
    public List<ClienteDto> UltimosClientes { get; private set; } = new();
    public List<ProductoDto> Alertas { get; private set; } = new();
    public List<Categoria> PorCategoria { get; private set; } = new();
    public List<(string Rol, int Cantidad)> UsuariosPorRol { get; private set; } = new();
    public List<Permiso> PermisosRol { get; private set; } = new();

    public int TotalClientes { get; private set; }
    public int TotalProductos { get; private set; }
    public int Unidades { get; private set; }
    public int Agotados { get; private set; }
    public decimal ValorInventario { get; private set; }
    public int UsuariosActivos { get; private set; }
    public int TotalUsuarios { get; private set; }

    public static string Moneda(decimal valor, string formato = "N2") => "RD$ " + valor.ToString(formato, CultureInfo.InvariantCulture);

    public async Task<IActionResult> OnGetAsync()
    {
        var clientes = await _api.ListarClientesAsync();
        var error = ManejarErrorApi(clientes);
        if (error is not null) return error;

        var productos = await _api.ListarProductosAsync();
        error = ManejarErrorApi(productos);
        if (error is not null) return error;

        var listaClientes = clientes.Datos ?? new();
        var listaProductos = productos.Datos ?? new();

        TotalClientes = listaClientes.Count;
        UltimosClientes = listaClientes.OrderByDescending(c => c.ClienteId).Take(4).ToList();

        TotalProductos = listaProductos.Count;
        Unidades = listaProductos.Sum(p => p.Stock);
        Agotados = listaProductos.Count(p => p.Stock == 0);
        ValorInventario = listaProductos.Sum(p => p.Precio * p.Stock);
        Alertas = listaProductos.Where(p => p.Stock <= StockBajo).OrderBy(p => p.Stock).ThenBy(p => p.Codigo).ToList();
        PorCategoria = listaProductos
            .GroupBy(p => p.Categoria)
            .Select(g => new Categoria(g.Key, g.Count(), g.Sum(p => p.Stock), g.Sum(p => p.Precio * p.Stock)))
            .OrderByDescending(c => c.Valor)
            .ToList();

        // Solo el rol que gestiona usuarios pide esa información (la API se la negaría a los demás)
        VeUsuarios = Puede(Acciones.CrearUsuarios);
        if (VeUsuarios)
        {
            var usuarios = await _api.ListarUsuariosAsync();
            error = ManejarErrorApi(usuarios);
            if (error is not null) return error;

            var listaUsuarios = usuarios.Datos ?? new();
            TotalUsuarios = listaUsuarios.Count;
            UsuariosActivos = listaUsuarios.Count(u => u.EstadoCuenta);
            UsuariosPorRol = listaUsuarios.GroupBy(u => u.Rol).OrderBy(g => g.Key).Select(g => (g.Key, g.Count())).ToList();
        }
        else
        {
            // Para los demás roles: qué puede hacer su rol (permisos en vivo)
            PermisosRol = new[] { Acciones.Agregar, Acciones.Modificar, Acciones.Eliminar, Acciones.Consultar }
                .Select(a => new Permiso(NombresPermiso.Descripcion(a), Puede(a)))
                .ToList();
        }

        return Page();
    }
}
