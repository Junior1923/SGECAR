using System.ComponentModel.DataAnnotations;
using System.Globalization;
using SGECAR.Shared.Contracts;

namespace SGECAR.App.Pages.Productos;

// Campos del formulario de producto 
public class DatosProducto
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9._-]+$", ErrorMessage = "Solo letras, números, punto o guiones (ej. PROD-004).")]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = "";

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = "";

    // Se recibe como texto para aceptar siempre el punto decimal (850.00),
   
    [Required(ErrorMessage = "El precio es obligatorio.")]
    [RegularExpression(@"^\d{1,13}(\.\d{1,2})?$", ErrorMessage = "Escriba un número mayor o igual a 0, con hasta 2 decimales (ej. 850.50).")]
    [Display(Name = "Precio (RD$)")]
    public string Precio { get; set; } = "";

    [Required(ErrorMessage = "El stock es obligatorio.")]
    [Range(0, 1_000_000, ErrorMessage = "El stock debe ser un número entero entre 0 y 1,000,000.")]
    [Display(Name = "Stock")]
    public int? Stock { get; set; }

    [Required(ErrorMessage = "Seleccione una categoría.")]
    [Display(Name = "Categoría")]
    public int? CategoriaId { get; set; }

    public static DatosProducto Desde(ProductoDto p) => new()
    {
        Codigo = p.Codigo,
        Nombre = p.Nombre,
        Precio = p.Precio.ToString("0.00", CultureInfo.InvariantCulture),
        Stock = p.Stock,
        CategoriaId = p.CategoriaId
    };

    public ProductoRequest ARequest() => new()
    {
        Codigo = Codigo.Trim().ToUpperInvariant(),
        Nombre = Nombre.Trim(),
        Precio = decimal.Parse(Precio, CultureInfo.InvariantCulture),
        Stock = Stock!.Value,
        CategoriaId = CategoriaId!.Value
    };
}
