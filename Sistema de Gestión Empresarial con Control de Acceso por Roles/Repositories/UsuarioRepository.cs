using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Sistema_de_Gestión_Empresarial_con_Control_de_Acceso_por_Roles.Data;
using Sistema_de_Gestión_Empresarial_con_Control_de_Acceso_por_Roles.Models;

namespace Sistema_de_Gestión_Empresarial_con_Control_de_Acceso_por_Roles.Repositories;

public class UsuarioRepository
{
    private readonly GestionEmpresarialContext _context;

    public UsuarioRepository(GestionEmpresarialContext context)
    {
        _context = context;
    }

    // VALIDAR USUARIO
    public async Task<Usuario?> UserValidate(
        string usuario,
        string contrasena)
    {
        var user = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Usuario1 == usuario);

        if (user == null)
            return null;

        if (!user.EstadoCuenta)
            return null;

        using var sha512 = SHA512.Create();

        byte[] bytes = Encoding.UTF8.GetBytes(contrasena);
        byte[] hash = sha512.ComputeHash(bytes);

        string hashIngresado = Convert.ToHexString(hash);

        if (user.ContrasenaHash != hashIngresado)
            return null;

        return user;
    }

    // REGISTRAR INTENTO FALLIDO
    public async Task RegisterFailAttempt(string usuario)
    {
        var user = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Usuario1 == usuario);

        if (user == null)
            return;

        user.IntentosFallidos++;

        if (user.IntentosFallidos >= 3)
        {
            user.EstadoCuenta = false;
        }

        await _context.SaveChangesAsync();
    }

    // REINICIAR INTENTOS FALLIDOS
    public async Task ResetFailAttempts(Usuario user)
    {
        user.IntentosFallidos = 0;

        await _context.SaveChangesAsync();
    }

    // OBTENER ROL DEL USUARIO
    public async Task<string?> GetRoleByUser(int usuarioId)
    {
        var user = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

        if (user == null)
            return null;

        return user.Rol?.Nombre;
    }

    // CREAR USUARIO
    public async Task<bool> CreateUser(
     string usuario,
     string contrasena,
     int rolId)
    {
        try
        {
            using var sha512 = SHA512.Create();

            byte[] bytes = Encoding.UTF8.GetBytes(contrasena);
            byte[] hash = sha512.ComputeHash(bytes);

            string contrasenaHash = Convert.ToHexString(hash);

            var nuevoUsuario = new Usuario
            {
                Usuario1 = usuario,
                ContrasenaHash = contrasenaHash,
                RolId = rolId,
                IntentosFallidos = 0,
                EstadoCuenta = true
            };

            _context.Usuarios.Add(nuevoUsuario);

            await _context.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }
    public async Task<List<Usuario>> ListUsers()
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
            .ToListAsync();
    }

}