using System.Security.Cryptography;
using System.Text;

namespace TestingSQLServerCnx.Services;

public class EncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public EncryptionService()
    {
        // Clave única basada en la máquina - solo funcionará en esta máquina
        var machineKey = Environment.MachineName + Environment.UserName + "SQLConnTester2025";
        using var sha256 = SHA256.Create();
        _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(machineKey)).Take(32).ToArray();
        _iv = sha256.ComputeHash(Encoding.UTF8.GetBytes(machineKey + "IV")).Take(16).ToArray();
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }
            } // CryptoStream se cierra aquí, forzando el flush de todos los datos
            
            return Convert.ToBase64String(msEncrypt.ToArray());
        }
        catch (Exception ex)
        {
            // Mostrar el error para debugging
            Console.WriteLine($"[ERROR] Encriptación falló: {ex.Message}");
            return string.Empty; // Devolver vacío en lugar del texto plano por seguridad
        }
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        try
        {
            var buffer = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var decryptor = aes.CreateDecryptor();
            using var msDecrypt = new MemoryStream(buffer);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            
            return srDecrypt.ReadToEnd();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Desencriptación falló: {ex.Message}");
            return string.Empty; // Devolver vacío en lugar del texto cifrado por seguridad
        }
    }

    public bool IsEncrypted(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        try
        {
            Convert.FromBase64String(text);
            // Si es Base64 válido y se puede desencriptar, probablemente está encriptado
            var decrypted = Decrypt(text);
            return decrypted != text; // Si es diferente, está encriptado
        }
        catch
        {
            return false;
        }
    }
}