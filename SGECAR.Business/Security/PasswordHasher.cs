using System.Security.Cryptography;
using System.Text;

namespace SGECAR.Business.Security
{
    // HASH DE CONTRASEÑAS: PBKDF2 CON HMAC-SHA256 Y SALT ALEATORIO POR USUARIO
    // FORMATO ALMACENADO: PBKDF2-SHA256$<iteraciones>$<salt hex>$<hash hex>  (118 caracteres)
    public static class PasswordHasher
    {
        private const string Algoritmo = "PBKDF2-SHA256";
        private const int Iteraciones = 100_000;
        private const int TamanoSalt = 16;
        private const int TamanoHash = 32;

        public static string Hash(string contrasena)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(TamanoSalt);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                contrasena, salt, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);

            return $"{Algoritmo}${Iteraciones}${Convert.ToHexString(salt)}${Convert.ToHexString(hash)}";
        }

        // requiereRehash = true CUANDO LA CONTRASEÑA ES CORRECTA PERO ESTÁ EN UN FORMATO ANTIGUO
        public static bool Verify(string contrasena, string almacenado, out bool requiereRehash)
        {
            requiereRehash = false;

            if (string.IsNullOrEmpty(almacenado))
                return false;

            var partes = almacenado.Split('$');

            if (partes.Length == 4 && partes[0] == Algoritmo)
            {
                if (!int.TryParse(partes[1], out int iteraciones) || iteraciones <= 0)
                    return false;

                if (!TryFromHex(partes[2], out byte[] salt) || !TryFromHex(partes[3], out byte[] esperado))
                    return false;

                byte[] calculado = Rfc2898DeriveBytes.Pbkdf2(
                    contrasena, salt, iteraciones, HashAlgorithmName.SHA256, esperado.Length);

                bool valido = CryptographicOperations.FixedTimeEquals(calculado, esperado);
                requiereRehash = valido && iteraciones < Iteraciones;
                return valido;
            }

            // FORMATO LEGADO: SHA-512 SIN SALT (USUARIOS SEMBRADOS POR GestionEmpresarial.sql).
            // SI LA CONTRASEÑA ES CORRECTA SE MARCA PARA MIGRARLA AL FORMATO CON SALT.
            if (almacenado.Length == 128 && TryFromHex(almacenado, out byte[] legado))
            {
                byte[] calculado = SHA512.HashData(Encoding.UTF8.GetBytes(contrasena));

                bool valido = CryptographicOperations.FixedTimeEquals(calculado, legado);
                requiereRehash = valido;
                return valido;
            }

            return false;
        }

        private static bool TryFromHex(string texto, out byte[] bytes)
        {
            try
            {
                bytes = Convert.FromHexString(texto);
                return bytes.Length > 0;
            }
            catch (FormatException)
            {
                bytes = Array.Empty<byte>();
                return false;
            }
        }
    }
}
