using TestingSQLServerCnx.Services;

namespace TestingSQLServerCnx;

public class TestEncryption
{
    public static void RunTest()
    {
        Console.WriteLine("=== PRUEBA DE ENCRIPTACIÓN ===");
        
        try
        {
            Console.WriteLine("Creando servicio de encriptación...");
            var encryptionService = new EncryptionService();
            Console.WriteLine("✓ Servicio creado exitosamente");
            
            var testPassword = "mi_contraseña_de_prueba";
            
            Console.WriteLine($"Contraseña original: {testPassword}");
            Console.WriteLine($"Longitud original: {testPassword.Length}");
            
            Console.WriteLine("\nIniciando encriptación...");
            var encrypted = encryptionService.Encrypt(testPassword);
            Console.WriteLine($"Contraseña encriptada: '{encrypted ?? "NULL"}'");
            Console.WriteLine($"Longitud encriptada: {encrypted?.Length ?? 0}");
            
            if (!string.IsNullOrEmpty(encrypted))
            {
                Console.WriteLine("\nIniciando desencriptación...");
                var decrypted = encryptionService.Decrypt(encrypted);
                Console.WriteLine($"Contraseña desencriptada: '{decrypted ?? "NULL"}'");
                Console.WriteLine($"Longitud desencriptada: {decrypted?.Length ?? 0}");
                
                var isWorking = testPassword == decrypted;
                Console.WriteLine($"¿Funciona la encriptación? {isWorking}");
                
                if (!isWorking)
                {
                    Console.WriteLine($"ERROR: Las contraseñas no coinciden");
                    Console.WriteLine($"Original: '{testPassword}'");
                    Console.WriteLine($"Recuperada: '{decrypted ?? "NULL"}'");
                }
                else
                {
                    Console.WriteLine("✓ ¡Encriptación funcionando correctamente!");
                }
            }
            else
            {
                Console.WriteLine("ERROR: La encriptación devolvió una cadena vacía o nula");
                Console.WriteLine("Revisa los mensajes de error anteriores");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR CRÍTICO en prueba de encriptación: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
        }
        
        Console.WriteLine("=== FIN PRUEBA ===");
    }
}