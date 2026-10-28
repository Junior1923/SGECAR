using Microsoft.AspNetCore.Mvc;
using SGECAR.API.Security;
using SGECAR.Shared.Contracts;
using SGECAR.Shared.Contracts.Services;
using SGECAR.Shared.Security;

namespace SGECAR.API.Controllers
{
    [Route("api/clientes")]
    public class ClientesController : ApiControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        // LISTAR / CONSULTAR CLIENTES (buscar: NOMBRE, APELLIDO, TELÉFONO O CORREO)
        [HttpGet]
        [RequierePermiso(Acciones.Consultar)]
        [ProducesResponseType(typeof(List<ClienteDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] string? buscar)
        {
            var clientes = await _clienteService.ListClientsAsync(buscar);
            return Ok(clientes.Select(ToDto));
        }

        // OBTENER UN CLIENTE
        [HttpGet("{id:int}")]
        [RequierePermiso(Acciones.Consultar)]
        [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Obtener(int id)
        {
            var cliente = await _clienteService.GetClientAsync(id);

            if (cliente == null)
                return NotFound(new MensajeResponse("El cliente no existe."));

            return Ok(ToDto(cliente));
        }

        // CREAR CLIENTE
        [HttpPost]
        [RequierePermiso(Acciones.Agregar)]
        [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Crear([FromBody] ClienteRequest request)
        {
            var resultado = await _clienteService.CreateClientAsync(request);

            if (!resultado.Exitoso)
                return Resultado(resultado);

            var cliente = await _clienteService.GetClientAsync(resultado.Id!.Value);
            return CreatedAtAction(nameof(Obtener), new { id = resultado.Id }, ToDto(cliente!));
        }

        // MODIFICAR CLIENTE
        [HttpPut("{id:int}")]
        [RequierePermiso(Acciones.Modificar)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Modificar(int id, [FromBody] ClienteRequest request)
        {
            return Resultado(await _clienteService.UpdateClientAsync(id, request));
        }

        // ELIMINAR CLIENTE
        [HttpDelete("{id:int}")]
        [RequierePermiso(Acciones.Eliminar)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Eliminar(int id)
        {
            return Resultado(await _clienteService.DeleteClientAsync(id));
        }
    }
}
