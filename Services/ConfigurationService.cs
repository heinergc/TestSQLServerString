using Newtonsoft.Json;
using TestingSQLServerCnx.Models;

namespace TestingSQLServerCnx.Services;

public class ConfigurationService
{
    private readonly string _configFile;
    private readonly EncryptionService _encryptionService;
    private List<ConnectionConfiguration> _configurations;

    public ConfigurationService()
    {
        _encryptionService = new EncryptionService();
        _configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "connections.json");
        _configurations = LoadConfigurations();
    }

    public List<ConnectionConfiguration> GetAllConfigurations()
    {
        return _configurations.ToList();
    }

    public ConnectionConfiguration? GetConfiguration(string id)
    {
        return _configurations.FirstOrDefault(c => c.Id == id);
    }

    public void AddConfiguration(ConnectionConfiguration configuration)
    {
        configuration.Id = Guid.NewGuid().ToString();
        configuration.CreatedAt = DateTime.Now;
        
        // Encriptar contraseña si no está vacía y no usa autenticación integrada
        if (!configuration.IntegratedSecurity && !string.IsNullOrEmpty(configuration.Password))
        {
            configuration.Password = _encryptionService.Encrypt(configuration.Password);
            configuration.IsPasswordEncrypted = true;
        }
        
        _configurations.Add(configuration);
        SaveConfigurations();
    }

    public void UpdateConfiguration(ConnectionConfiguration configuration)
    {
        var index = _configurations.FindIndex(c => c.Id == configuration.Id);
        if (index >= 0)
        {
            // Encriptar contraseña si no está vacía y no usa autenticación integrada
            if (!configuration.IntegratedSecurity && !string.IsNullOrEmpty(configuration.Password))
            {
                // Solo encriptar si no está ya encriptada o si viene en texto plano
                if (!configuration.IsPasswordEncrypted || !IsLikelyEncrypted(configuration.Password))
                {
                    configuration.Password = _encryptionService.Encrypt(configuration.Password);
                    configuration.IsPasswordEncrypted = true;
                }
            }
            else if (configuration.IntegratedSecurity)
            {
                // Para autenticación integrada, limpiar contraseña
                configuration.Password = string.Empty;
                configuration.IsPasswordEncrypted = false;
            }
            
            _configurations[index] = configuration;
            SaveConfigurations();
        }
    }

    public void DeleteConfiguration(string id)
    {
        _configurations.RemoveAll(c => c.Id == id);
        SaveConfigurations();
    }

    public string GetDecryptedPassword(string configurationId)
    {
        var config = GetConfiguration(configurationId);
        if (config == null || string.IsNullOrEmpty(config.Password))
            return string.Empty;

        if (config.IsPasswordEncrypted)
        {
            return _encryptionService.Decrypt(config.Password);
        }
        
        return config.Password;
    }

    public ConnectionConfiguration GetConfigurationWithDecryptedPassword(string id)
    {
        var config = GetConfiguration(id);
        if (config != null && config.IsPasswordEncrypted && !string.IsNullOrEmpty(config.Password))
        {
            var decryptedConfig = JsonConvert.DeserializeObject<ConnectionConfiguration>(JsonConvert.SerializeObject(config))!;
            decryptedConfig.Password = _encryptionService.Decrypt(config.Password);
            return decryptedConfig;
        }
        
        return config!;
    }

    public void ForceUpdatePassword(string configurationId, string newPassword)
    {
        Console.WriteLine($"[DEBUG] === INICIANDO ForceUpdatePassword ===");
        Console.WriteLine($"[DEBUG] ID: {configurationId}");
        Console.WriteLine($"[DEBUG] Nueva contraseña recibida: '{newPassword}' (longitud: {newPassword?.Length ?? 0})");
        
        var config = GetConfiguration(configurationId);
        if (config == null)
        {
            Console.WriteLine($"[DEBUG] ERROR: No se encontró la configuración con ID {configurationId}");
            return;
        }
        
        if (string.IsNullOrEmpty(newPassword))
        {
            Console.WriteLine($"[DEBUG] ERROR: La nueva contraseña está vacía o es nula");
            return;
        }

        Console.WriteLine($"[DEBUG] Configuración encontrada: {config.Name}");
        Console.WriteLine($"[DEBUG] Contraseña actual en config: '{config.Password}' (longitud: {config.Password?.Length ?? 0})");
        Console.WriteLine($"[DEBUG] IsPasswordEncrypted actual: {config.IsPasswordEncrypted}");

        try
        {
            // Limpiar estado previo
            config.IsPasswordEncrypted = false;
            
            // Encriptar la nueva contraseña
            Console.WriteLine($"[DEBUG] Iniciando encriptación...");
            var encryptedPassword = _encryptionService.Encrypt(newPassword);
            Console.WriteLine($"[DEBUG] Resultado encriptación: '{encryptedPassword}' (longitud: {encryptedPassword?.Length ?? 0})");
            
            if (!string.IsNullOrEmpty(encryptedPassword))
            {
                config.Password = encryptedPassword;
                config.IsPasswordEncrypted = true;
                
                Console.WriteLine($"[DEBUG] Actualizando configuración en memoria...");
                var index = _configurations.FindIndex(c => c.Id == configurationId);
                if (index >= 0)
                {
                    _configurations[index] = config;
                    Console.WriteLine($"[DEBUG] Configuración actualizada en índice {index}");
                    
                    Console.WriteLine($"[DEBUG] Guardando en archivo...");
                    SaveConfigurations();
                    Console.WriteLine($"[DEBUG] ✓ Configuración guardada exitosamente para {config.Name}");
                }
                else
                {
                    Console.WriteLine($"[DEBUG] ERROR: No se encontró el índice de la configuración");
                }
            }
            else
            {
                Console.WriteLine($"[DEBUG] ERROR: La encriptación devolvió una cadena vacía o nula");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] ERROR en ForceUpdatePassword: {ex.Message}");
            Console.WriteLine($"[DEBUG] StackTrace: {ex.StackTrace}");
        }
        
        Console.WriteLine($"[DEBUG] === FIN ForceUpdatePassword ===");
    }

    public void EncryptExistingPasswords()
    {
        bool hasChanges = false;
        
        foreach (var config in _configurations)
        {
            if (!config.IntegratedSecurity && !string.IsNullOrEmpty(config.Password) && !config.IsPasswordEncrypted)
            {
                // Solo encriptar si la contraseña no está vacía y no contiene datos Base64 ya
                if (!IsLikelyEncrypted(config.Password))
                {
                    config.Password = _encryptionService.Encrypt(config.Password);
                    config.IsPasswordEncrypted = true;
                    hasChanges = true;
                }
            }
        }
        
        if (hasChanges)
        {
            SaveConfigurations();
        }
    }

    private bool IsLikelyEncrypted(string password)
    {
        // Verificar si la contraseña parece ser Base64 (datos encriptados)
        if (string.IsNullOrEmpty(password))
            return false;
            
        try
        {
            Convert.FromBase64String(password);
            return password.Length > 20; // Las contraseñas encriptadas suelen ser más largas
        }
        catch
        {
            return false;
        }
    }

    public void UpdateTestResult(string id, ConnectionTestResult result)
    {
        var config = GetConfiguration(id);
        if (config != null)
        {
            config.LastTested = DateTime.Now;
            config.LastTestResult = result;
            SaveConfigurations();
        }
    }

    private List<ConnectionConfiguration> LoadConfigurations()
    {
        try
        {
            if (File.Exists(_configFile))
            {
                var json = File.ReadAllText(_configFile);
                return JsonConvert.DeserializeObject<List<ConnectionConfiguration>>(json) ?? new List<ConnectionConfiguration>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando configuraciones: {ex.Message}");
        }

        return new List<ConnectionConfiguration>();
    }

    private void SaveConfigurations()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_configFile)!);
            var json = JsonConvert.SerializeObject(_configurations, Formatting.Indented);
            File.WriteAllText(_configFile, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error guardando configuraciones: {ex.Message}");
        }
    }
}