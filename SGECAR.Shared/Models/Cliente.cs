namespace SGECAR.Shared.Models;

public partial class Cliente
{
    public int ClienteId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string? Telefono { get; set; }

    public string Correo { get; set; } = null!;

    public string? Direccion { get; set; }
}
