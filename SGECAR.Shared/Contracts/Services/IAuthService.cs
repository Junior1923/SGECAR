namespace SGECAR.Shared.Contracts.Services;

public interface IAuthService
{
    // VERIFICA CREDENCIALES, REGISTRA INTENTOS FALLIDOS Y BLOQUEA LA CUENTA AL LLEGAR AL MÁXIMO
    Task<LoginResultado> LoginAsync(string? usuario, string? contrasena);
}
