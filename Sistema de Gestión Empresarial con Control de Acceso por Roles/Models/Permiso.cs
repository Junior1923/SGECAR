using System;
using System.Collections.Generic;

namespace Sistema_de_Gestión_Empresarial_con_Control_de_Acceso_por_Roles.Models;

public partial class Permiso
{
    public int PermisoId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<Role> Rols { get; set; } = new List<Role>();
}
