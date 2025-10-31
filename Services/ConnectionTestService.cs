using Microsoft.Data.SqlClient;
using System.Diagnostics;
using TestingSQLServerCnx.Models;

namespace TestingSQLServerCnx.Services;

public class ConnectionTestService
{
    private readonly EncryptionService _encryptionService;

    public ConnectionTestService()
    {
        _encryptionService = new EncryptionService();
    }

    public async Task<ConnectionTestResult> TestConnectionAsync(ConnectionConfiguration config)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            using var connection = new SqlConnection(config.GetConnectionString(_encryptionService));
            
            await connection.OpenAsync();
            
            // Obtener información del servidor
            string? serverVersion = null;
            string? databaseName = null;
            
            try
            {
                using var command = new SqlCommand("SELECT @@VERSION, DB_NAME()", connection);
                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    serverVersion = reader.GetString(0).Split('\n')[0].Trim();
                    databaseName = reader.GetString(1);
                }
            }
            catch
            {
                // Si no podemos obtener la información adicional, no es crítico
            }
            
            stopwatch.Stop();
            return ConnectionTestResult.Success(stopwatch.Elapsed, serverVersion, databaseName);
        }
        catch (SqlException sqlEx)
        {
            stopwatch.Stop();
            string message = sqlEx.Number switch
            {
                2 => "No se puede conectar al servidor SQL Server. Verifica que el servidor esté ejecutándose y que el nombre sea correcto.",
                18456 => "Error de autenticación. Verifica el usuario y contraseña.",
                53 => "No se pudo establecer conexión con el servidor. Verifica la dirección del servidor y el puerto.",
                4060 => "No se puede abrir la base de datos especificada.",
                _ => $"Error SQL ({sqlEx.Number}): {sqlEx.Message}"
            };
            
            return ConnectionTestResult.Failure(message, sqlEx, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return ConnectionTestResult.Failure($"Error inesperado: {ex.Message}", ex, stopwatch.Elapsed);
        }
    }

    public async Task<List<string>> GetAvailableDatabasesAsync(ConnectionConfiguration config)
    {
        var databases = new List<string>();
        
        try
        {
            // Crear una configuración temporal sin especificar base de datos
            var tempConfig = new ConnectionConfiguration
            {
                Server = config.Server,
                Username = config.Username,
                Password = config.Password,
                IntegratedSecurity = config.IntegratedSecurity,
                ConnectionTimeout = config.ConnectionTimeout,
                CommandTimeout = config.CommandTimeout,
                TrustServerCertificate = config.TrustServerCertificate,
                Database = "master" // Conectar a master para listar bases de datos
            };

            using var connection = new SqlConnection(tempConfig.GetConnectionString());
            await connection.OpenAsync();
            
            using var command = new SqlCommand("SELECT name FROM sys.databases WHERE state = 0 ORDER BY name", connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                databases.Add(reader.GetString(0));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error obteniendo bases de datos: {ex.Message}");
        }

        return databases;
    }

    public async Task<bool> TestQueryAsync(ConnectionConfiguration config, string query)
    {
        try
        {
            using var connection = new SqlConnection(config.GetConnectionString(_encryptionService));
            await connection.OpenAsync();
            
            using var command = new SqlCommand(query, connection);
            command.CommandTimeout = config.CommandTimeout;
            
            var result = await command.ExecuteScalarAsync();
            Console.WriteLine($"Resultado de la consulta: {result}");
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error ejecutando consulta: {ex.Message}");
            return false;
        }
    }
}