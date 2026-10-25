using System.Net.Http.Headers;
using System.Text.Json;
using SGECAR.Shared.Contracts;

namespace SGECAR.App.Servicios;

// Todas las llamadas de la App a SGECAR.API pasan por esta clase
public class ApiCliente
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _contexto;

    public ApiCliente(IHttpClientFactory fabrica, IHttpContextAccessor contexto)
    {
        _http = fabrica.CreateClient("SGECAR.API");
        _contexto = contexto;
    }

    private ISession Sesion => _contexto.HttpContext!.Session;

    // ---------- Autenticación ----------

    public async Task<RespuestaApi<LoginResponse>> LoginAsync(string usuario, string contrasena)
    {
        var respuesta = await EnviarAsync<LoginResponse>(HttpMethod.Post, "api/auth/login",
            new LoginRequest { Usuario = usuario, Contrasena = contrasena });

        if (respuesta.Exitoso && respuesta.Datos is not null)
        {
            Sesion.Guardar(respuesta.Datos.Token, respuesta.Datos.Sesion);
            respuesta.Mensaje = respuesta.Datos.Mensaje;
        }

        return respuesta;
    }

    public async Task LogoutAsync()
    {
        if (Sesion.ObtenerToken() is not null)
            await EnviarAsync<MensajeResponse>(HttpMethod.Post, "api/auth/logout");

        Sesion.Cerrar();
    }

    // ---------- Usuarios ----------

    public Task<RespuestaApi<List<UsuarioDto>>> ListarUsuariosAsync() =>
        EnviarAsync<List<UsuarioDto>>(HttpMethod.Get, "api/usuarios");

    public Task<RespuestaApi<UsuarioDto>> ObtenerUsuarioAsync(int id) =>
        EnviarAsync<UsuarioDto>(HttpMethod.Get, $"api/usuarios/{id}");

    public Task<RespuestaApi<UsuarioDto>> CrearUsuarioAsync(UsuarioCrearRequest datos) =>
        EnviarAsync<UsuarioDto>(HttpMethod.Post, "api/usuarios", datos);

    public Task<RespuestaApi<MensajeResponse>> ModificarUsuarioAsync(int id, UsuarioActualizarRequest datos) =>
        EnviarAsync<MensajeResponse>(HttpMethod.Put, $"api/usuarios/{id}", datos);

    public Task<RespuestaApi<MensajeResponse>> DesbloquearUsuarioAsync(int id) =>
        EnviarAsync<MensajeResponse>(HttpMethod.Post, $"api/usuarios/{id}/desbloquear");

    public Task<RespuestaApi<MensajeResponse>> EliminarUsuarioAsync(int id) =>
        EnviarAsync<MensajeResponse>(HttpMethod.Delete, $"api/usuarios/{id}");

    // ---------- Roles y permisos ----------

    public Task<RespuestaApi<List<RolDto>>> ListarRolesAsync() =>
        EnviarAsync<List<RolDto>>(HttpMethod.Get, "api/roles");

    public Task<RespuestaApi<RolDto>> CrearRolAsync(RolRequest datos) =>
        EnviarAsync<RolDto>(HttpMethod.Post, "api/roles", datos);

    public Task<RespuestaApi<List<PermisoDto>>> ListarPermisosAsync() =>
        EnviarAsync<List<PermisoDto>>(HttpMethod.Get, "api/permisos");

    // ---------- Envío común ----------

    private async Task<RespuestaApi<T>> EnviarAsync<T>(HttpMethod metodo, string ruta, object? cuerpo = null)
    {
        using var peticion = new HttpRequestMessage(metodo, ruta);

        var token = Sesion.ObtenerToken();
        if (token is not null)
            peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (cuerpo is not null)
            peticion.Content = JsonContent.Create(cuerpo);

        try
        {
            using var respuesta = await _http.SendAsync(peticion);
            var resultado = new RespuestaApi<T>
            {
                Codigo = (int)respuesta.StatusCode,
                Exitoso = respuesta.IsSuccessStatusCode
            };

            if (respuesta.IsSuccessStatusCode)
                resultado.Datos = await respuesta.Content.ReadFromJsonAsync<T>();
            else
                resultado.Mensaje = await LeerMensajeErrorAsync(respuesta);

            return resultado;
        }
        catch (HttpRequestException)
        {
            return new RespuestaApi<T>
            {
                Codigo = 0,
                Mensaje = "No se pudo conectar con la API. Verifique que SGECAR.API esté en ejecución."
            };
        }
    }

    // La API devuelve { "mensaje": "..." } o, en validaciones, { "errors": { "Campo": ["..."] } }
    private static async Task<string> LeerMensajeErrorAsync(HttpResponseMessage respuesta)
    {
        try
        {
            using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
            var raiz = json.RootElement;

            if (raiz.TryGetProperty("mensaje", out var mensaje))
                return mensaje.GetString() ?? string.Empty;

            if (raiz.TryGetProperty("errors", out var errores))
                return string.Join(" ", errores.EnumerateObject()
                    .SelectMany(e => e.Value.EnumerateArray())
                    .Select(e => e.GetString()));
        }
        catch (JsonException)
        {
        }

        return (int)respuesta.StatusCode switch
        {
            401 => "Su sesión expiró. Inicie sesión nuevamente.",
            403 => "No tiene permisos para realizar esta acción.",
            404 => "El registro no existe.",
            _ => "Ocurrió un error al comunicarse con la API."
        };
    }
}
