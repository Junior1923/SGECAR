using Microsoft.EntityFrameworkCore;
using SGECAR.Data.Context;
using SGECAR.Shared.Models;

namespace SGECAR.Data.Repositories;

public class UsuarioRepository
{
    private readonly GestionEmpresarialContext _context;

    public UsuarioRepository(GestionEmpresarialContext context)
    {
        _context = context;
    }

    // OBTENER USUARIO POR NOMBRE (INCLUYE ROL Y PERMISOS PARA EL LOGIN)
    public async Task<Usuario?> GetUserByUsername(string usuario)
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
                .ThenInclude(r => r.Permisos)
            .FirstOrDefaultAsync(u => u.Usuario1 == usuario);
    }

    // OBTENER USUARIO POR ID
    public async Task<Usuario?> GetUserById(int usuarioId)
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);
    }

    // LISTAR USUARIOS
    public async Task<List<Usuario>> ListUsers()
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
            .OrderBy(u => u.Usuario1)
            .ToListAsync();
    }

    // VERIFICAR SI YA EXISTE UN NOMBRE DE USUARIO
    public async Task<bool> ExistsUsername(string usuario, int? excluirUsuarioId = null)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.Usuario1 == usuario && u.UsuarioId != excluirUsuarioId);
    }

    // CREAR USUARIO
    public async Task CreateUser(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
    }

    // ELIMINAR USUARIO
    public async Task DeleteUser(Usuario usuario)
    {
        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();
    }

    // GUARDAR CAMBIOS DE ENTIDADES YA RASTREADAS
    public async Task SaveChanges()
    {
        await _context.SaveChangesAsync();
    }
}
