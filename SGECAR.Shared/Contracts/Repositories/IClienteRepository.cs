using SGECAR.Shared.Models;

namespace SGECAR.Shared.Contracts.Repositories;

public interface IClienteRepository
{
    // LISTAR CLIENTES (buscar FILTRA POR NOMBRE, APELLIDO, TELÉFONO O CORREO)
    Task<List<Cliente>> ListClients(string? buscar = null);

    // OBTENER CLIENTE POR ID
    Task<Cliente?> GetClientById(int clienteId);

    // VERIFICAR SI YA EXISTE UN CORREO
    Task<bool> ExistsEmail(string correo, int? excluirClienteId = null);

    // CREAR CLIENTE
    Task CreateClient(Cliente cliente);

    // ELIMINAR CLIENTE
    Task DeleteClient(Cliente cliente);

    // GUARDAR CAMBIOS DE ENTIDADES YA RASTREADAS
    Task SaveChanges();
}
