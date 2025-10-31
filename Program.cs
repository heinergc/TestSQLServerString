using TestingSQLServerCnx.Services;

namespace TestingSQLServerCnx;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Configurar la consola para soporte de caracteres especiales
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "Probador de Conexiones SQL Server";

            // Inicializar servicios
            var configService = new ConfigurationService();
            var testService = new ConnectionTestService();
            var consoleService = new ConsoleService(configService, testService);

            // Encriptar contraseñas existentes si no están encriptadas
            configService.EncryptExistingPasswords();

            // Ejecutar la aplicación
            await consoleService.RunAsync();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nError crítico en la aplicación: {ex.Message}");
            Console.WriteLine("\nDetalles del error:");
            Console.WriteLine(ex.ToString());
            Console.ResetColor();
            
            Console.WriteLine("\nPresiona cualquier tecla para salir...");
            Console.ReadKey();
            
            Environment.Exit(1);
        }
    }
}