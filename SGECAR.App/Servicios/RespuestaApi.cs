namespace SGECAR.App.Servicios;

// Resultado de una llamada a la API
public class RespuestaApi<T>
{
    public bool Exitoso { get; set; }

    // Código HTTP: 200, 201, 400, 401, 403, 404, 409, 423... (0 = la API no respondió)
    public int Codigo { get; set; }

    public string Mensaje { get; set; } = string.Empty;

    public T? Datos { get; set; }

    public bool SesionVencida => Codigo == 401;
    public bool SinPermiso => Codigo == 403;
    public bool CuentaBloqueada => Codigo == 423;
}