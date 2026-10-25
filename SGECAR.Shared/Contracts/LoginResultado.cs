namespace SGECAR.Shared.Contracts;

public enum EstadoLogin
{
    Exitoso,
    DatosIncompletos,
    CredencialesInvalidas,
    CuentaBloqueada
}

public class LoginResultado
{
    public EstadoLogin Estado { get; init; }

    public string Mensaje { get; init; } = string.Empty;

    public SesionUsuario? Sesion { get; init; }

    public bool Exitoso => Estado == EstadoLogin.Exitoso;
}
