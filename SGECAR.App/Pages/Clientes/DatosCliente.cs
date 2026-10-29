using System.ComponentModel.DataAnnotations;
using SGECAR.Shared.Contracts;

namespace SGECAR.App.Pages.Clientes;

// Campos del formulario de cliente (los usan Crear y Editar).
// Se validan aquí para mostrar el error antes de enviar; la API vuelve a validar.
public class DatosCliente
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ .'-]+$", ErrorMessage = "Solo letras y espacios.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ .'-]+$", ErrorMessage = "Solo letras y espacios.")]
    [Display(Name = "Apellido")]
    public string Apellido { get; set; } = "";

    [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
    [RegularExpression(@"^[0-9()+\- ]{7,20}$", ErrorMessage = "Use solo números, espacios o guiones (ej. 809-555-0101).")]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [StringLength(254, ErrorMessage = "Máximo 254 caracteres.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [Display(Name = "Correo")]
    public string Correo { get; set; } = "";

    [StringLength(250, ErrorMessage = "Máximo 250 caracteres.")]
    [Display(Name = "Dirección")]
    public string? Direccion { get; set; }

    public static DatosCliente Desde(ClienteDto c) => new()
    {
        Nombre = c.Nombre,
        Apellido = c.Apellido,
        Telefono = c.Telefono,
        Correo = c.Correo,
        Direccion = c.Direccion
    };

    // Lo que se envía a la API (sin espacios sobrantes y con vacíos como null)
    public ClienteRequest ARequest() => new()
    {
        Nombre = Nombre.Trim(),
        Apellido = Apellido.Trim(),
        Telefono = string.IsNullOrWhiteSpace(Telefono) ? null : Telefono.Trim(),
        Correo = Correo.Trim(),
        Direccion = string.IsNullOrWhiteSpace(Direccion) ? null : Direccion.Trim()
    };
}
