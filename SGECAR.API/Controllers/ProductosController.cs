using Microsoft.AspNetCore.Mvc;
using SGECAR.API.Security;
using SGECAR.Shared.Contracts;
using SGECAR.Shared.Contracts.Services;
using SGECAR.Shared.Security;

namespace SGECAR.API.Controllers
{
    [Route("api/productos")]
    public class ProductosController : ApiControllerBase
    {
        private readonly IProductoService _productoService;

        public ProductosController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        // LISTAR / CONSULTAR PRODUCTOS (buscar: CÓDIGO O NOMBRE; categoriaId: FILTRO OPCIONAL)
        [HttpGet]
        [RequierePermiso(Acciones.Consultar)]
        [ProducesResponseType(typeof(List<ProductoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] string? buscar, [FromQuery] int? categoriaId)
        {
            var productos = await _productoService.ListProductsAsync(buscar, categoriaId);
            return Ok(productos.Select(ToDto));
        }

        // OBTENER UN PRODUCTO
        [HttpGet("{id:int}")]
        [RequierePermiso(Acciones.Consultar)]
        [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Obtener(int id)
        {
            var producto = await _productoService.GetProductAsync(id);

            if (producto == null)
                return NotFound(new MensajeResponse("El producto no existe."));

            return Ok(ToDto(producto));
        }

        // LISTAR CATEGORÍAS (PARA LA LISTA DESPLEGABLE DEL FORMULARIO)
        [HttpGet("categorias")]
        [RequierePermiso(Acciones.Consultar)]
        [ProducesResponseType(typeof(List<CategoriaDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarCategorias()
        {
            var categorias = await _productoService.ListCategoriesAsync();
            return Ok(categorias.Select(ToDto));
        }

        // CREAR PRODUCTO
        [HttpPost]
        [RequierePermiso(Acciones.Agregar)]
        [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Crear([FromBody] ProductoRequest request)
        {
            var resultado = await _productoService.CreateProductAsync(request);

            if (!resultado.Exitoso)
                return Resultado(resultado);

            var producto = await _productoService.GetProductAsync(resultado.Id!.Value);
            return CreatedAtAction(nameof(Obtener), new { id = resultado.Id }, ToDto(producto!));
        }

        // MODIFICAR PRODUCTO
        [HttpPut("{id:int}")]
        [RequierePermiso(Acciones.Modificar)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Modificar(int id, [FromBody] ProductoRequest request)
        {
            return Resultado(await _productoService.UpdateProductAsync(id, request));
        }

        // ELIMINAR PRODUCTO
        [HttpDelete("{id:int}")]
        [RequierePermiso(Acciones.Eliminar)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Eliminar(int id)
        {
            return Resultado(await _productoService.DeleteProductAsync(id));
        }
    }
}
