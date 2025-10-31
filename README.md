# Probador de Conexiones SQL Server

Una aplicación de consola desarrollada en C# .NET 9 para probar y gestionar múltiples conexiones a SQL Server. Ideal para desarrolladores y administradores que necesitan verificar conectividad a diferentes servidores SQL Server en entornos de desarrollo, pruebas y producción.

## 🚀 Características

- **Gestión de múltiples conexiones**: Guarda y administra diferentes configuraciones de conexión
- **Pruebas de conectividad**: Verifica conexiones individuales o todas a la vez
- **Autenticación flexible**: Soporte para autenticación de Windows e integrada
- **Métricas de rendimiento**: Mide el tiempo de respuesta de cada conexión
- **Consultas personalizadas**: Ejecuta consultas SQL de prueba
- **Interfaz intuitiva**: Menú interactivo fácil de usar
- **Almacenamiento seguro**: Configuraciones guardadas en formato JSON

## 📋 Requisitos

- .NET 9.0 o superior
- Windows (para autenticación integrada de Windows)
- Acceso a una o más instancias de SQL Server

## 🛠️ Instalación

1. Clona o descarga el proyecto
2. Restaura las dependencias:
   ```bash
   dotnet restore
   ```
3. Compila el proyecto:
   ```bash
   dotnet build
   ```
4. Ejecuta la aplicación:
   ```bash
   dotnet run
   ```

## 📚 Uso

### Menú Principal

La aplicación presenta un menú interactivo con las siguientes opciones:

1. **📋 Listar conexiones** - Muestra todas las conexiones configuradas con su estado
2. **➕ Agregar nueva conexión** - Configura una nueva conexión SQL Server
3. **🔌 Probar una conexión** - Prueba una conexión específica
4. **🔌 Probar todas las conexiones** - Ejecuta pruebas en todas las conexiones
5. **✏️ Editar conexión** - Modifica una conexión existente
6. **🗑️ Eliminar conexión** - Elimina una conexión de la lista
7. **📝 Ejecutar consulta personalizada** - Ejecuta una consulta SQL en una conexión
0. **🚪 Salir** - Cierra la aplicación

### Configuración de Conexiones

Al agregar una nueva conexión, puedes especificar:

- **Nombre**: Identificador amigable para la conexión
- **Servidor**: Dirección del servidor SQL (localhost, IP, nombre\\instancia)
- **Base de datos**: Base de datos específica (opcional)
- **Autenticación**: Windows Integrada o SQL Server
- **Credenciales**: Usuario y contraseña (solo para autenticación SQL)
- **Timeouts**: Configuración de timeouts de conexión y comando

### Ejemplos de Servidores

```
localhost                    # Instancia local predeterminada
localhost\\SQLEXPRESS        # Instancia con nombre
192.168.1.100               # Servidor por IP
servidor.empresa.com        # Servidor por nombre
servidor.empresa.com,1433   # Servidor con puerto específico
```

### Tipos de Autenticación

**Autenticación de Windows (Integrada)**
- Utiliza las credenciales del usuario actual de Windows
- No requiere usuario/contraseña
- Recomendada para entornos de dominio

**Autenticación SQL Server**
- Utiliza credenciales específicas de SQL Server
- Requiere usuario y contraseña
- Útil para aplicaciones web y conexiones remotas

## 📁 Estructura del Proyecto

```
TestingSQLServerCnx/
├── Data/
│   ├── connections.json         # Configuraciones guardadas
│   └── connections.example.json # Configuraciones de ejemplo
├── Models/
│   ├── ConnectionConfiguration.cs
│   └── ConnectionTestResult.cs
├── Services/
│   ├── ConfigurationService.cs  # Gestión de configuraciones
│   ├── ConnectionTestService.cs # Pruebas de conectividad
│   └── ConsoleService.cs        # Interfaz de usuario
├── Program.cs                   # Punto de entrada
└── TestingSQLServerCnx.csproj  # Archivo del proyecto
```

## 🔧 Configuración

Las configuraciones se almacenan en `Data/connections.json`. Ejemplo:

```json
[
  {
    "Id": "unique-id",
    "Name": "Mi Servidor Local",
    "Server": "localhost",
    "Database": "MiBaseDeDatos",
    "Username": "",
    "Password": "",
    "IntegratedSecurity": true,
    "ConnectionTimeout": 30,
    "CommandTimeout": 30,
    "TrustServerCertificate": true,
    "CreatedAt": "2024-01-01T00:00:00",
    "LastTested": null,
    "LastTestResult": null
  }
]
```

## 🛡️ Seguridad

- Las contraseñas se almacenan en texto plano en el archivo JSON
- Para entornos de producción, considera usar:
  - Variables de entorno para credenciales sensibles
  - Azure Key Vault u otros servicios de gestión de secretos
  - Cifrado de archivos de configuración

## 🐛 Solución de Problemas

### Errores Comunes

**Error 2**: No se puede conectar al servidor
- Verifica que SQL Server esté ejecutándose
- Confirma el nombre del servidor y puerto
- Revisa la configuración del firewall

**Error 18456**: Error de autenticación
- Verifica usuario y contraseña
- Confirma que el usuario tenga permisos
- Revisa si está habilitada la autenticación SQL Server

**Error 53**: No se pudo establecer conexión
- Verifica la dirección del servidor
- Confirma que los protocolos estén habilitados
- Revisa la configuración de red

**Error 4060**: No se puede abrir la base de datos
- Verifica que la base de datos exista
- Confirma permisos de acceso a la base de datos

## 🔄 Actualización

Para actualizar las dependencias:

```bash
dotnet restore
```

Para compilar en modo release:

```bash
dotnet build --configuration Release
```

## 📄 Licencia

Este proyecto es de código abierto y está disponible bajo la licencia MIT.

## 🤝 Contribuciones

Las contribuciones son bienvenidas. Por favor:

1. Fork el proyecto
2. Crea una rama para tu feature
3. Commit tus cambios
4. Push a la rama
5. Abre un Pull Request

## 📞 Soporte

Si encuentras algún problema o tienes sugerencias:

1. Revisa la sección de solución de problemas
2. Verifica que tienes la versión correcta de .NET
3. Confirma que SQL Server esté accesible
4. Abre un issue con detalles del problema