namespace SGECAR.Shared.Security;

// NOMBRES DE LOS PERMISOS (DEBEN COINCIDIR CON LA TABLA Permisos)
public static class Acciones
{
    public const string Agregar = "AGREGAR";
    public const string Modificar = "MODIFICAR";
    public const string Eliminar = "ELIMINAR";
    public const string Consultar = "CONSULTAR";
    public const string CrearUsuarios = "CREAR_USUARIOS";
    public const string CrearRoles = "CREAR_ROLES";

    public static readonly IReadOnlyList<string> Todas = new[]
    {
        Agregar, Modificar, Eliminar, Consultar, CrearUsuarios, CrearRoles
    };
}

// ROLES DEL SISTEMA
public static class RolesSistema
{
    // EL ROL ADMINISTRADOR NO SE PUEDE ELIMINAR NI MODIFICAR PARA EVITAR PERDER EL ACCESO
    public const string Administrador = "Administrador";
    public const string Supervisor = "Supervisor";
    public const string Ejecutor = "Ejecutor";
}
