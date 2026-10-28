using Microsoft.EntityFrameworkCore;
using SGECAR.Data.Context;
using SGECAR.Shared.Contracts.Repositories;
using SGECAR.Shared.Models;

namespace SGECAR.Data.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly GestionEmpresarialContext _context;

    public ClienteRepository(GestionEmpresarialContext context)
    {
        _context = context;
    }

    // LISTAR CLIENTES (buscar FILTRA POR NOMBRE, APELLIDO, TELÉFONO O CORREO)
    public async Task<List<Cliente>> ListClients(string? buscar = null)
    {
        var consulta = _context.Clientes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();

            // SIN DISTINGUIR ACENTOS: "perez" ENCUENTRA "Pérez"
            consulta = consulta.Where(c =>
                EF.Functions.Collate(c.Nombre + " " + c.Apellido, Busqueda.Intercalacion).Contains(buscar) ||
                (c.Telefono != null && c.Telefono.Contains(buscar)) ||
                EF.Functions.Collate(c.Correo, Busqueda.Intercalacion).Contains(buscar));
        }

        return await consulta
            .OrderBy(c => c.Nombre)
            .ThenBy(c => c.Apellido)
            .ToListAsync();
    }

    // OBTENER CLIENTE POR ID
    public async Task<Cliente?> GetClientById(int clienteId)
    {
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId);
    }

    // VERIFICAR SI YA EXISTE UN CORREO
    public async Task<bool> ExistsEmail(string correo, int? excluirClienteId = null)
    {
        return await _context.Clientes
            .AnyAsync(c => c.Correo == correo && c.ClienteId != excluirClienteId);
    }

    // CREAR CLIENTE
    public async Task CreateClient(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
    }

    // ELIMINAR CLIENTE
    public async Task DeleteClient(Cliente cliente)
    {
        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
    }

    // GUARDAR CAMBIOS DE ENTIDADES YA RASTREADAS
    public async Task SaveChanges()
    {
        await _context.SaveChangesAsync();
    }
}
