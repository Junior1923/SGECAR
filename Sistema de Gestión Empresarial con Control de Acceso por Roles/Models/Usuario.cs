using System;
using System.Collections.Generic;

namespace Sistema_de_Gestión_Empresarial_con_Control_de_Acceso_por_Roles.Models;

public partial class Usuario
{
    public int UsuarioId { get; set; }

    public string Usuario1 { get; set; } = null!;

    public string ContrasenaHash { get; set; } = null!;

    public int RolId { get; set; }

    public int IntentosFallidos { get; set; }

    public bool EstadoCuenta { get; set; }

    public virtual Role Rol { get; set; } = null!;
}
