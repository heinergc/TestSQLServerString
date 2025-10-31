using TestingSQLServerCnx.Services;

namespace TestingSQLServerCnx.Models;

public class ConnectionConfiguration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsPasswordEncrypted { get; set; } = false;
    public bool IntegratedSecurity { get; set; } = false;
    public int ConnectionTimeout { get; set; } = 30;
    public int CommandTimeout { get; set; } = 30;
    public bool TrustServerCertificate { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LastTested { get; set; }
    public ConnectionTestResult? LastTestResult { get; set; }

    public string GetConnectionString(EncryptionService? encryptionService = null)
    {
        var actualPassword = Password;
        
        // Si la contraseña está encriptada y tenemos el servicio de encriptación, desencriptarla
        if (IsPasswordEncrypted && !string.IsNullOrEmpty(Password) && encryptionService is not null)
        {
            Console.WriteLine($"[DEBUG] Desencriptando contraseña para {Name}...");
            Console.WriteLine($"[DEBUG] Contraseña encriptada: {Password.Length} caracteres");
            actualPassword = encryptionService.Decrypt(Password);
            Console.WriteLine($"[DEBUG] Contraseña desencriptada: {actualPassword?.Length ?? 0} caracteres");
        }
        else
        {
            Console.WriteLine($"[DEBUG] No se desencriptó: IsEncrypted={IsPasswordEncrypted}, HasPassword={!string.IsNullOrEmpty(Password)}, HasService={encryptionService is not null}");
        }

        if (IntegratedSecurity)
        {
            return $"Server={Server};Database={Database};Integrated Security=true;Connection Timeout={ConnectionTimeout};Command Timeout={CommandTimeout};TrustServerCertificate={TrustServerCertificate};";
        }
        else
        {
            return $"Server={Server};Database={Database};User Id={Username};Password={actualPassword};Connection Timeout={ConnectionTimeout};Command Timeout={CommandTimeout};TrustServerCertificate={TrustServerCertificate};";
        }
    }

    public override string ToString()
    {
        return $"{Name} ({Server}) - {(IntegratedSecurity ? "Windows Auth" : "SQL Auth")}";
    }
}