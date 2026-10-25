namespace SGECAR.Shared.Contracts;

public enum TipoError
{
    Ninguno,
    Validacion,
    NoEncontrado,
    Conflicto
}

public class OperacionResultado
{
    public bool Exitoso { get; init; }

    public string Mensaje { get; init; } = string.Empty;

    public TipoError TipoError { get; init; }

    // ID DEL REGISTRO CREADO (SOLO EN OPERACIONES DE ALTA)
    public int? Id { get; init; }

    public static OperacionResultado Ok(string mensaje, int? id = null) =>
        new() { Exitoso = true, Mensaje = mensaje, Id = id };

    public static OperacionResultado Error(string mensaje, TipoError tipo = TipoError.Validacion) =>
        new() { Exitoso = false, Mensaje = mensaje, TipoError = tipo };

    public static OperacionResultado NoEncontrado(string mensaje) => Error(mensaje, TipoError.NoEncontrado);

    public static OperacionResultado Conflicto(string mensaje) => Error(mensaje, TipoError.Conflicto);
}
