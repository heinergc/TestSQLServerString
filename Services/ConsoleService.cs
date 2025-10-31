using TestingSQLServerCnx.Models;

namespace TestingSQLServerCnx.Services;

public class ConsoleService
{
    private readonly ConfigurationService _configService;
    private readonly ConnectionTestService _testService;
    private readonly ExcelReportService _excelService;

    public ConsoleService(ConfigurationService configService, ConnectionTestService testService)
    {
        _configService = configService;
        _testService = testService;
        _excelService = new ExcelReportService();
    }

    public async Task RunAsync()
    {
        Console.Clear();
        ShowWelcome();

        while (true)
        {
            ShowMainMenu();
            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await ListConnectionsAsync();
                        break;
                    case "2":
                        await AddConnectionAsync();
                        break;
                    case "3":
                        await TestConnectionAsync();
                        break;
                    case "4":
                        await TestAllConnectionsAsync();
                        break;
                    case "5":
                        await EditConnectionAsync();
                        break;
                    case "6":
                        await DeleteConnectionAsync();
                        break;
                    case "7":
                        await TestCustomQueryAsync();
                        break;
                    case "8":
                        await ViewPasswordsAsync();
                        break;
                    case "9":
                        await FixCorruptedPasswordsAsync();
                        break;
                    case "99":
                        TestEncryption.RunTest();
                        Console.WriteLine("\nPresiona cualquier tecla para continuar...");
                        Console.ReadKey();
                        break;
                    case "10":
                        await GenerateExcelReportAsync();
                        break;
                    case "0":
                        Console.WriteLine("\n¡Hasta luego!");
                        return;
                    default:
                        Console.WriteLine("\nOpción no válida. Presiona cualquier tecla para continuar...");
                        Console.ReadKey();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                Console.WriteLine("Presiona cualquier tecla para continuar...");
                Console.ReadKey();
            }
        }
    }

    private void ShowWelcome()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║          PROBADOR DE CONEXIONES SQL SERVER v1.0             ║");
        Console.WriteLine("║             Desarrollado con .NET 9                         ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
    }

    private void ShowMainMenu()
    {
        Console.Clear();
        ShowWelcome();
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══ MENÚ PRINCIPAL ═══");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("1. 📋 Listar conexiones");
        Console.WriteLine("2. ➕ Agregar nueva conexión");
        Console.WriteLine("3. 🔌 Probar una conexión");
        Console.WriteLine("4. 🔌 Probar todas las conexiones");
        Console.WriteLine("5. ✏️  Editar conexión");
        Console.WriteLine("6. 🗑️  Eliminar conexión");
        Console.WriteLine("7. 📝 Ejecutar consulta personalizada");
        Console.WriteLine("8. 🔐 Ver contraseñas (solo lectura)");
        Console.WriteLine("9. 🔧 Reparar contraseñas corruptas");
        Console.WriteLine("10. 📊 Generar reporte Excel");
        Console.WriteLine("99. 🧪 Probar encriptación (debug)");
        Console.WriteLine("0. 🚪 Salir");
        Console.WriteLine();
        Console.Write("Selecciona una opción: ");
    }

    private async Task ListConnectionsAsync()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══ CONEXIONES CONFIGURADAS ═══");
        Console.ResetColor();
        Console.WriteLine();

        var configurations = _configService.GetAllConfigurations();
        
        if (!configurations.Any())
        {
            Console.WriteLine("No hay conexiones configuradas.");
        }
        else
        {
            Console.WriteLine($"{"#",-3} {"Nombre",-20} {"Servidor",-25} {"Base de Datos",-15} {"Tipo Auth",-12} {"Último Test",-15}");
            Console.WriteLine(new string('─', 95));

            for (int i = 0; i < configurations.Count; i++)
            {
                var config = configurations[i];
                var authType = config.IntegratedSecurity ? "Windows" : "SQL Server";
                var lastTest = config.LastTested?.ToString("dd/MM/yy HH:mm") ?? "Nunca";
                
                Console.ForegroundColor = config.LastTestResult?.IsSuccessful == true ? ConsoleColor.Green : 
                                         config.LastTestResult?.IsSuccessful == false ? ConsoleColor.Red : ConsoleColor.Gray;
                
                Console.WriteLine($"{i + 1,-3} {config.Name,-20} {config.Server,-25} {config.Database,-15} {authType,-12} {lastTest,-15}");
                Console.ResetColor();
                
                if (config.LastTestResult != null)
                {
                    Console.WriteLine($"    └─ {config.LastTestResult}");
                }
            }
        }

        Console.WriteLine("\nPresiona cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private async Task AddConnectionAsync()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══ AGREGAR NUEVA CONEXIÓN ═══");
        Console.ResetColor();
        Console.WriteLine();

        var config = new ConnectionConfiguration();

        Console.Write("Nombre de la conexión: ");
        config.Name = Console.ReadLine() ?? "";

        Console.Write("Servidor (ej: localhost, servidor\\instancia, ip:puerto): ");
        config.Server = Console.ReadLine() ?? "";

        Console.Write("Base de datos (opcional): ");
        config.Database = Console.ReadLine() ?? "";

        Console.Write("¿Usar autenticación integrada de Windows? (s/n): ");
        config.IntegratedSecurity = Console.ReadLine()?.ToLower().StartsWith("s") ?? false;

        if (!config.IntegratedSecurity)
        {
            Console.Write("Usuario: ");
            config.Username = Console.ReadLine() ?? "";

            Console.Write("Contraseña: ");
            config.Password = ReadPassword();
        }

        Console.Write("Timeout de conexión en segundos (30): ");
        if (int.TryParse(Console.ReadLine(), out int connTimeout))
            config.ConnectionTimeout = connTimeout;

        Console.Write("Timeout de comando en segundos (30): ");
        if (int.TryParse(Console.ReadLine(), out int cmdTimeout))
            config.CommandTimeout = cmdTimeout;

        _configService.AddConfiguration(config);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Conexión agregada exitosamente!");
        Console.ResetColor();

        Console.Write("\n¿Deseas probar la conexión ahora? (s/n): ");
        if (Console.ReadLine()?.ToLower().StartsWith("s") ?? false)
        {
            await TestSpecificConnectionAsync(config);
        }

        Console.WriteLine("\nPresiona cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private async Task TestConnectionAsync()
    {
        var configurations = _configService.GetAllConfigurations();
        if (!configurations.Any())
        {
            Console.WriteLine("\nNo hay conexiones configuradas.");
            Console.WriteLine("Presiona cualquier tecla para continuar...");
            Console.ReadKey();
            return;
        }

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══ PROBAR CONEXIÓN ═══");
        Console.ResetColor();
        Console.WriteLine();

        for (int i = 0; i < configurations.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {configurations[i]}");
        }

        Console.Write("\nSelecciona el número de conexión a probar: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= configurations.Count)
        {
            await TestSpecificConnectionAsync(configurations[index - 1]);
        }
        else
        {
            Console.WriteLine("Selección no válida.");
        }

        Console.WriteLine("\nPresiona cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private async Task TestSpecificConnectionAsync(ConnectionConfiguration config)
    {
        Console.WriteLine($"\nProbando conexión: {config.Name}...");
        Console.WriteLine("Por favor espera...");

        var result = await _testService.TestConnectionAsync(config);
        _configService.UpdateTestResult(config.Id, result);

        Console.WriteLine();
        if (result.IsSuccessful)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ CONEXIÓN EXITOSA");
            Console.WriteLine($"  Tiempo de respuesta: {result.ResponseTime.TotalMilliseconds:F0}ms");
            if (!string.IsNullOrEmpty(result.SqlServerVersion))
                Console.WriteLine($"  Versión del servidor: {result.SqlServerVersion}");
            if (!string.IsNullOrEmpty(result.DatabaseName))
                Console.WriteLine($"  Base de datos: {result.DatabaseName}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ CONEXIÓN FALLÓ");
            Console.WriteLine($"  Error: {result.Message}");
            Console.WriteLine($"  Tiempo transcurrido: {result.ResponseTime.TotalMilliseconds:F0}ms");
        }
        Console.ResetColor();
    }

    private async Task TestAllConnectionsAsync()
    {
        var configurations = _configService.GetAllConfigurations();
        if (!configurations.Any())
        {
            Console.WriteLine("\nNo hay conexiones configuradas.");
            Console.WriteLine("Presiona cualquier tecla para continuar...");
            Console.ReadKey();
            return;
        }

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══ PROBANDO TODAS LAS CONEXIONES ═══");
        Console.ResetColor();
        Console.WriteLine();

        int successful = 0;
        int failed = 0;

        foreach (var config in configurations)
        {
            Console.Write($"Probando {config.Name}... ");
            
            var result = await _testService.TestConnectionAsync(config);
            _configService.UpdateTestResult(config.Id, result);

            if (result.IsSuccessful)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ OK ({result.ResponseTime.TotalMilliseconds:F0}ms)");
                successful++;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ FALLÓ - {result.Message}");
                failed++;
            }
            Console.ResetColor();
        }

        Console.WriteLine();
        Console.WriteLine($"Resumen: {successful} exitosas, {failed} fallidas de {configurations.Count} total");
        Console.WriteLine("\nPresiona cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private async Task EditConnectionAsync()
    {
        var configurations = _configService.GetAllConfigurations();
        if (!configurations.Any())
        {
            Console.WriteLine("\nNo hay conexiones configuradas.");
            Console.WriteLine("Presiona cualquier tecla para continuar...");
            Console.ReadKey();
            return;
        }

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══ EDITAR CONEXIÓN ═══");
        Console.ResetColor();
        Console.WriteLine();

        for (int i = 0; i < configurations.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {configurations[i]}");
        }

        Console.Write("\nSelecciona el número de conexión a editar: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= configurations.Count)
        {
            var config = configurations[index - 1];
            
            Console.WriteLine($"\nEditando: {config.Name}");
            Console.WriteLine("(Presiona Enter para mantener el valor actual)");
            Console.WriteLine();

            Console.Write($"Nombre [{config.Name}]: ");
            var newName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newName)) config.Name = newName;

            Console.Write($"Servidor [{config.Server}]: ");
            var newServer = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newServer)) config.Server = newServer;

            Console.Write($"Base de datos [{config.Database}]: ");
            var newDatabase = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newDatabase)) config.Database = newDatabase;

            Console.Write($"¿Usar autenticación integrada? [{(config.IntegratedSecurity ? "Sí" : "No")}] (s/n): ");
            var authResponse = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(authResponse))
                config.IntegratedSecurity = authResponse.ToLower().StartsWith("s");

            if (!config.IntegratedSecurity)
            {
                Console.Write($"Usuario [{config.Username}]: ");
                var newUsername = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newUsername)) config.Username = newUsername;

                Console.Write("Nueva contraseña (dejar vacío para no cambiar): ");
                var newPassword = ReadPassword();
                if (!string.IsNullOrWhiteSpace(newPassword)) config.Password = newPassword;
            }

            _configService.UpdateConfiguration(config);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n✓ Conexión actualizada exitosamente!");
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine("Selección no válida.");
        }

        Console.WriteLine("\nPresiona cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private async Task DeleteConnectionAsync()
    {
        var configurations = _configService.GetAllConfigurations();
        if (!configurations.Any())
        {
            Console.WriteLine("\nNo hay conexiones configuradas.");
            Console.WriteLine("Presiona cualquier tecla para continuar...");
            Console.ReadKey();
            return;
        }

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══ ELIMINAR CONEXIÓN ═══");
        Console.ResetColor();
        Console.WriteLine();

        for (int i = 0; i < configurations.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {configurations[i]}");
        }

        Console.Write("\nSelecciona el número de conexión a eliminar: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= configurations.Count)
        {
            var config = configurations[index - 1];
            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"\n¿Estás seguro de eliminar '{config.Name}'? (s/n): ");
            Console.ResetColor();
            
            if (Console.ReadLine()?.ToLower().StartsWith("s") ?? false)
            {
                _configService.DeleteConfiguration(config.Id);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Conexión eliminada exitosamente!");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("Eliminación cancelada.");
            }
        }
        else
        {
            Console.WriteLine("Selección no válida.");
        }

        Console.WriteLine("\nPresiona cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private async Task TestCustomQueryAsync()
    {
        var configurations = _configService.GetAllConfigurations();
        if (!configurations.Any())
        {
            Console.WriteLine("\nNo hay conexiones configuradas.");
            Console.WriteLine("Presiona cualquier tecla para continuar...");
            Console.ReadKey();
            return;
        }

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══ EJECUTAR CONSULTA PERSONALIZADA ═══");
        Console.ResetColor();
        Console.WriteLine();

        for (int i = 0; i < configurations.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {configurations[i]}");
        }

        Console.Write("\nSelecciona la conexión: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= configurations.Count)
        {
            var config = configurations[index - 1];
            
            Console.WriteLine("\nEjemplos de consultas:");
            Console.WriteLine("  SELECT @@VERSION");
            Console.WriteLine("  SELECT GETDATE()");
            Console.WriteLine("  SELECT DB_NAME()");
            Console.WriteLine("  SELECT COUNT(*) FROM sys.tables");
            Console.WriteLine();
            
            Console.Write("Ingresa tu consulta SQL: ");
            var query = Console.ReadLine();
            
            if (!string.IsNullOrWhiteSpace(query))
            {
                Console.WriteLine("\nEjecutando consulta...");
                var success = await _testService.TestQueryAsync(config, query);
                
                if (success)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ Consulta ejecutada exitosamente!");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("✗ Error ejecutando la consulta.");
                }
                Console.ResetColor();
            }
        }
        else
        {
            Console.WriteLine("Selección no válida.");
        }

        Console.WriteLine("\nPresiona cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private string ReadPassword()
    {
        var password = "";
        ConsoleKeyInfo key;
        
        do
        {
            key = Console.ReadKey(true);
            
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[..^1];
                Console.Write("\b \b");
            }
        }
        while (key.Key != ConsoleKey.Enter);
        
        Console.WriteLine();
        return password;
    }

    private async Task ViewPasswordsAsync()
    {
        Console.Clear();
        ShowWelcome();
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("═══ VER CONTRASEÑAS (SOLO LECTURA) ═══");
        Console.ResetColor();
        Console.WriteLine();
        
        var configs = _configService.GetAllConfigurations();
        if (!configs.Any())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("No hay conexiones configuradas.");
            Console.ResetColor();
            Console.WriteLine("\nPresiona cualquier tecla para continuar...");
            Console.ReadKey();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("⚠️  ADVERTENCIA DE SEGURIDAD:");
        Console.WriteLine("Las contraseñas se mostrarán en texto plano.");
        Console.WriteLine("Asegúrate de que nadie más pueda ver tu pantalla.");
        Console.ResetColor();
        Console.WriteLine();
        Console.Write("¿Deseas continuar? (s/N): ");
        
        var confirm = Console.ReadLine()?.ToLower();
        if (confirm != "s" && confirm != "si")
        {
            Console.WriteLine("Operación cancelada.");
            Console.WriteLine("\nPresiona cualquier tecla para continuar...");
            Console.ReadKey();
            return;
        }

        Console.Clear();
        ShowWelcome();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("═══ CONTRASEÑAS DESENCRIPTADAS ═══");
        Console.ResetColor();
        Console.WriteLine();

        for (int i = 0; i < configs.Count; i++)
        {
            var config = configs[i];
            
            Console.WriteLine($"{i + 1}. {config.Name}");
            Console.WriteLine($"   Servidor: {config.Server}");
            Console.WriteLine($"   Base de datos: {config.Database}");
            Console.WriteLine($"   Usuario: {config.Username}");
            
            if (config.IntegratedSecurity)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("   Autenticación: Windows (Sin contraseña)");
                Console.ResetColor();
            }
            else
            {
                var decryptedPassword = _configService.GetDecryptedPassword(config.Id);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"   Contraseña: {decryptedPassword}");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"   Estado: {(config.IsPasswordEncrypted ? "Encriptada en archivo" : "Texto plano en archivo")}");
                Console.ResetColor();
            }
            
            Console.WriteLine($"   Creada: {config.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            
            if (config.LastTested.HasValue)
            {
                Console.WriteLine($"   Última prueba: {config.LastTested:yyyy-MM-dd HH:mm:ss}");
                if (config.LastTestResult != null)
                {
                    var color = config.LastTestResult.IsSuccessful ? ConsoleColor.Green : ConsoleColor.Red;
                    Console.ForegroundColor = color;
                    Console.WriteLine($"   Resultado: {config.LastTestResult.Message}");
                    Console.ResetColor();
                }
            }
            
            Console.WriteLine();
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Las contraseñas mostradas arriba están desencriptadas temporalmente.");
        Console.WriteLine("En el archivo JSON siguen estando encriptadas y seguras.");
        Console.ResetColor();
        Console.WriteLine("\nPresiona cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private async Task FixCorruptedPasswordsAsync()
    {
        Console.Clear();
        ShowWelcome();
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══ REPARAR CONTRASEÑAS CORRUPTAS ═══");
        Console.ResetColor();
        Console.WriteLine();
        
        var configs = _configService.GetAllConfigurations();
        var corruptedConfigs = configs.Where(c => !c.IntegratedSecurity && 
                                                 (string.IsNullOrEmpty(c.Password) || 
                                                  (c.IsPasswordEncrypted && string.IsNullOrEmpty(c.Password)))).ToList();
        
        if (!corruptedConfigs.Any())
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ No se encontraron contraseñas corruptas.");
            Console.ResetColor();
            Console.WriteLine("\nPresiona cualquier tecla para continuar...");
            Console.ReadKey();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Se encontraron {corruptedConfigs.Count} conexiones con contraseñas corruptas:");
        Console.ResetColor();
        Console.WriteLine();

        foreach (var config in corruptedConfigs)
        {
            Console.WriteLine($"• {config.Name} ({config.Server})");
        }

        Console.WriteLine();
        Console.WriteLine("Deberás ingresar nuevamente las contraseñas para estas conexiones.");
        Console.WriteLine();
        Console.Write("¿Deseas continuar? (s/N): ");
        
        var confirm = Console.ReadLine()?.ToLower();
        if (confirm != "s" && confirm != "si")
        {
            Console.WriteLine("Operación cancelada.");
            Console.WriteLine("\nPresiona cualquier tecla para continuar...");
            Console.ReadKey();
            return;
        }

        foreach (var config in corruptedConfigs)
        {
            Console.Clear();
            ShowWelcome();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"═══ REPARANDO: {config.Name} ═══");
            Console.ResetColor();
            Console.WriteLine();
            
            Console.WriteLine($"Servidor: {config.Server}");
            Console.WriteLine($"Base de datos: {config.Database}");
            Console.WriteLine($"Usuario: {config.Username}");
            Console.WriteLine();
            
            Console.Write("Ingresa la nueva contraseña: ");
            var newPassword = ReadPassword();
            
            if (!string.IsNullOrEmpty(newPassword))
            {
                _configService.ForceUpdatePassword(config.Id, newPassword);
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✓ Contraseña actualizada para {config.Name}");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n⚠️ Saltando {config.Name} (contraseña vacía)");
                Console.ResetColor();
            }
            
            Console.WriteLine("\nPresiona cualquier tecla para continuar...");
            Console.ReadKey();
        }

        Console.Clear();
        ShowWelcome();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✓ Proceso de reparación completado.");
        Console.ResetColor();
        Console.WriteLine("\nPresiona cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private async Task GenerateExcelReportAsync()
    {
        Console.Clear();
        ShowWelcome();
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("═══ GENERAR REPORTE EXCEL ═══");
        Console.ResetColor();
        Console.WriteLine();
        
        var configs = _configService.GetAllConfigurations();
        if (!configs.Any())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("No hay conexiones configuradas para generar el reporte.");
            Console.ResetColor();
            Console.WriteLine("\nPresiona cualquier tecla para continuar...");
            Console.ReadKey();
            return;
        }

        Console.WriteLine($"Se generará un reporte Excel con {configs.Count} conexiones.");
        Console.WriteLine();
        Console.Write("¿Deseas continuar? (s/N): ");
        
        var confirm = Console.ReadLine()?.ToLower();
        if (confirm != "s" && confirm != "si")
        {
            Console.WriteLine("Operación cancelada.");
            Console.WriteLine("\nPresiona cualquier tecla para continuar...");
            Console.ReadKey();
            return;
        }

        try
        {
            Console.WriteLine("\nGenerando reporte Excel...");
            Console.WriteLine("⏳ Procesando datos...");
            
            var filePath = _excelService.GenerateConnectionsReport(configs);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ ¡Reporte generado exitosamente!");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"📁 Ubicación del archivo:");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"   {filePath}");
            Console.ResetColor();
            Console.WriteLine();
            
            Console.WriteLine("📋 El reporte incluye:");
            Console.WriteLine("   • Lista completa de conexiones");
            Console.WriteLine("   • Estados de las últimas pruebas");
            Console.WriteLine("   • Tiempos de respuesta");
            Console.WriteLine("   • Información del servidor SQL");
            Console.WriteLine("   • Estadísticas generales");
            Console.WriteLine();
            
            Console.Write("¿Deseas abrir la carpeta del reporte? (s/N): ");
            var openFolder = Console.ReadLine()?.ToLower();
            if (openFolder == "s" || openFolder == "si")
            {
                try
                {
                    var directory = Path.GetDirectoryName(filePath);
                    System.Diagnostics.Process.Start("explorer.exe", directory!);
                    Console.WriteLine("✓ Carpeta abierta en el explorador.");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"No se pudo abrir la carpeta: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Error generando el reporte Excel:");
            Console.WriteLine($"   {ex.Message}");
            Console.ResetColor();
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Detalle: {ex.InnerException.Message}");
            }
        }
        
        Console.WriteLine("\nPresiona cualquier tecla para continuar...");
        Console.ReadKey();
    }
}