using System;
using System.Collections.Generic;

namespace SGECAR.Shared.Models;

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
