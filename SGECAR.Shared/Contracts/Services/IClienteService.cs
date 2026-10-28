using SGECAR.Shared.Models;

namespace SGECAR.Shared.Contracts.Services;

public interface IClienteService
{
    Task<List<Cliente>> ListClientsAsync(string? buscar = null);

    Task<Cliente?> GetClientAsync(int clienteId);

    Task<OperacionResultado> CreateClientAsync(ClienteRequest datos);

    Task<OperacionResultado> UpdateClientAsync(int clienteId, ClienteRequest datos);

    Task<OperacionResultado> DeleteClientAsync(int clienteId);
}
