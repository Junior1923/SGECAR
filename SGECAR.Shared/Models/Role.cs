using System;
using System.Collections.Generic;

namespace Sistema_de_Gestión_Empresarial_con_Control_de_Acceso_por_Roles.Models;

public partial class Role
{
    public int RolId { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

    public virtual ICollection<Permiso> Permisos { get; set; } = new List<Permiso>();
}
