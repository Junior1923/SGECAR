using System.Net.Mail;
using SGECAR.Shared.Contracts;
using SGECAR.Shared.Contracts.Repositories;
using SGECAR.Shared.Contracts.Services;
using SGECAR.Shared.Models;

namespace SGECAR.Business.Services
{
    public class ClienteService : IClienteService
    {
        private const int LongitudMaximaNombre = 100;
        private const int LongitudMaximaTelefono = 20;
        private const int LongitudMaximaCorreo = 254;
        private const int LongitudMaximaDireccion = 250;

        private readonly IClienteRepository _clientes;

        public ClienteService(IClienteRepository clientes)
        {
            _clientes = clientes;
        }

        public Task<List<Cliente>> ListClientsAsync(string? buscar = null) => _clientes.ListClients(buscar);

        public Task<Cliente?> GetClientAsync(int clienteId) => _clientes.GetClientById(clienteId);

        public async Task<OperacionResultado> CreateClientAsync(ClienteRequest datos)
        {
            var error = await Validar(datos, null);
            if (error != null)
                return error;

            var cliente = new Cliente();
            AsignarDatos(cliente, datos);

            await _clientes.CreateClient(cliente);

            return OperacionResultado.Ok($"Cliente \"{NombreCompleto(cliente)}\" creado correctamente.", cliente.ClienteId);
        }

        public async Task<OperacionResultado> UpdateClientAsync(int clienteId, ClienteRequest datos)
        {
            var cliente = await _clientes.GetClientById(clienteId);

            if (cliente == null)
                return OperacionResultado.NoEncontrado("El cliente no existe.");

            var error = await Validar(datos, clienteId);
            if (error != null)
                return error;

            AsignarDatos(cliente, datos);

            await _clientes.SaveChanges();

            return OperacionResultado.Ok($"Cliente \"{NombreCompleto(cliente)}\" actualizado correctamente.");
        }

        public async Task<OperacionResultado> DeleteClientAsync(int clienteId)
        {
            var cliente = await _clientes.GetClientById(clienteId);

            if (cliente == null)
                return OperacionResultado.NoEncontrado("El cliente no existe.");

            await _clientes.DeleteClient(cliente);

            return OperacionResultado.Ok($"Cliente \"{NombreCompleto(cliente)}\" eliminado correctamente.");
        }

        private async Task<OperacionResultado?> Validar(ClienteRequest datos, int? clienteId)
        {
            if (string.IsNullOrWhiteSpace(datos.Nombre))
                return OperacionResultado.Error("El nombre es obligatorio.");

            if (datos.Nombre.Trim().Length > LongitudMaximaNombre)
                return OperacionResultado.Error($"El nombre no puede exceder {LongitudMaximaNombre} caracteres.");

            if (string.IsNullOrWhiteSpace(datos.Apellido))
                return OperacionResultado.Error("El apellido es obligatorio.");

            if (datos.Apellido.Trim().Length > LongitudMaximaNombre)
                return OperacionResultado.Error($"El apellido no puede exceder {LongitudMaximaNombre} caracteres.");

            var telefono = Opcional(datos.Telefono);
            if (telefono != null)
            {
                if (telefono.Length > LongitudMaximaTelefono)
                    return OperacionResultado.Error($"El teléfono no puede exceder {LongitudMaximaTelefono} caracteres.");

                // SOLO DÍGITOS Y LOS SEPARADORES HABITUALES: 809-555-0101, (809) 555 0101, +1 809...
                if (!telefono.Any(char.IsDigit) || !telefono.All(c => char.IsDigit(c) || " -+()".Contains(c)))
                    return OperacionResultado.Error("El teléfono solo puede contener números, espacios, guiones, paréntesis y +.");
            }

            if (string.IsNullOrWhiteSpace(datos.Correo))
                return OperacionResultado.Error("El correo es obligatorio.");

            var correo = datos.Correo.Trim();

            if (correo.Length > LongitudMaximaCorreo)
                return OperacionResultado.Error($"El correo no puede exceder {LongitudMaximaCorreo} caracteres.");

            if (!EsCorreoValido(correo))
                return OperacionResultado.Error("El correo no tiene un formato válido.");

            if (await _clientes.ExistsEmail(correo, clienteId))
                return OperacionResultado.Conflicto($"Ya existe un cliente con el correo \"{correo}\".");

            var direccion = Opcional(datos.Direccion);
            if (direccion != null && direccion.Length > LongitudMaximaDireccion)
                return OperacionResultado.Error($"La dirección no puede exceder {LongitudMaximaDireccion} caracteres.");

            return null;
        }

        private static void AsignarDatos(Cliente cliente, ClienteRequest datos)
        {
            cliente.Nombre = datos.Nombre.Trim();
            cliente.Apellido = datos.Apellido.Trim();
            cliente.Telefono = Opcional(datos.Telefono);
            cliente.Correo = datos.Correo.Trim();
            cliente.Direccion = Opcional(datos.Direccion);
        }

        // LOS CAMPOS OPCIONALES VACÍOS SE GUARDAN COMO NULL
        private static string? Opcional(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

        private static bool EsCorreoValido(string correo) =>
            MailAddress.TryCreate(correo, out var direccion)
            && direccion.Address == correo
            && direccion.Host.Contains('.');

        private static string NombreCompleto(Cliente cliente) => $"{cliente.Nombre} {cliente.Apellido}";
    }
}
