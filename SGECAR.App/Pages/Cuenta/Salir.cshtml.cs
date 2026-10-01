using Microsoft.AspNetCore.Mvc.RazorPages;
using SGECAR.App.Servicios;

namespace SGECAR.App.Pages.Cuenta;

public class SalirModel : PageModel
{
    private readonly ApiCliente _api;

    public SalirModel(ApiCliente api)
    {
        _api = api;
    }

    public async Task OnGetAsync() => await _api.LogoutAsync();
}
