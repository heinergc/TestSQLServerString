# Probador de Conexiones SQL Server

Una aplicación de consola desarrollada en C# .NET 6 para probar y gestionar múltiples conexiones a SQL Server con funciones avanzadas de seguridad y reportería. Ideal para desarrolladores y administradores que necesitan verificar conectividad a diferentes servidores SQL Server en entornos de desarrollo, pruebas y producción.

## 🚀 Características

- **Gestión de múltiples conexiones**: Guarda y administra diferentes configuraciones de conexión
- **Seguridad avanzada**: Encriptación AES-256 de contraseñas con claves específicas por máquina
- **Pruebas de conectividad**: Verifica conexiones individuales o todas a la vez con métricas detalladas
- **Autenticación flexible**: Soporte para autenticación de Windows e integrada
- **Métricas de rendimiento**: Mide el tiempo de respuesta de cada conexión
- **Reportes Excel**: Genera reportes profesionales en Excel con estadísticas y análisis
- **Consultas personalizadas**: Ejecuta consultas SQL de prueba
- **Interfaz intuitiva**: Menú interactivo con 10 opciones completas
- **Almacenamiento seguro**: Configuraciones guardadas en formato JSON con encriptación

## 📋 Requisitos

- .NET 6.0 LTS o superior
- Windows (para autenticación integrada de Windows)
- Acceso a una o más instancias de SQL Server
- Microsoft Excel (opcional, para visualizar reportes generados)

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
8. **🔐 Re-encriptar contraseñas** - Actualiza la encriptación de todas las contraseñas
9. **📊 Generar reporte Excel** - Crea un reporte profesional con todas las conexiones
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
│   ├── connections.json         # Configuraciones guardadas (excluido del repositorio)
│   └── connections.example.json # Plantilla de configuración
├── Models/
│   ├── ConnectionConfiguration.cs  # Modelo de configuración de conexión
│   └── ConnectionTestResult.cs     # Modelo de resultados de prueba
├── Services/
│   ├── ConfigurationService.cs     # Gestión de configuraciones JSON
│   ├── ConnectionTestService.cs    # Pruebas de conectividad SQL
│   ├── ConsoleService.cs           # Interfaz de usuario por consola
│   ├── EncryptionService.cs        # Encriptación AES-256 de contraseñas
│   └── ExcelReportService.cs       # Generación de reportes Excel
├── bin/Debug/net6.0/
│   ├── Data/                       # Datos de configuración en runtime
│   └── Reportes/                   # Reportes Excel generados
├── Program.cs                      # Punto de entrada principal
├── TestingSQLServerCnx.csproj     # Archivo del proyecto .NET 6
└── .gitignore                      # Protección de archivos sensibles
```

## 📦 Dependencias

### Framework Base
- **Microsoft.NET.Sdk** - SDK de .NET 6.0 LTS

### Conectividad y Configuración
- **Microsoft.Data.SqlClient 5.1.1** - Cliente moderno para SQL Server
- **Microsoft.Extensions.Configuration 6.0.1** - Sistema de configuración
- **Microsoft.Extensions.Configuration.Json 6.0.0** - Soporte JSON

### Utilidades
- **Newtonsoft.Json 13.0.3** - Serialización JSON avanzada
- **EPPlus 6.2.10** - Generación de archivos Excel profesionales

## 🔧 Configuración

Las configuraciones se almacenan en `Data/connections.json` con encriptación AES-256. Ejemplo:

```json
[
  {
    "Id": "unique-guid-id",
    "Name": "Mi Servidor Producción",
    "Server": "servidor.empresa.com",
    "Database": "BDProduccion",
    "Username": "usuario_aplicacion",
    "Password": "cE257InEFEUidYqvfp9Y1A==",
    "IsPasswordEncrypted": true,
    "IntegratedSecurity": false,
    "ConnectionTimeout": 30,
    "CommandTimeout": 30,
    "TrustServerCertificate": true,
    "CreatedAt": "2025-10-31T12:00:00-06:00",
    "LastTested": "2025-10-31T14:49:38-06:00",
    "LastTestResult": {
      "IsSuccessful": true,
      "Message": "Conexión exitosa",
      "ResponseTime": "00:00:00.3758190",
      "TestedAt": "2025-10-31T14:49:38-06:00",
      "SqlServerVersion": "Microsoft SQL Server 2022 (RTM-CU21)",
      "DatabaseName": "BDProduccion",
      "Exception": null
    }
  }
]
```

### Configuración Segura

**Encriptación Automática:**
- Las contraseñas se encriptan automáticamente con AES-256
- Claves específicas por máquina para mayor seguridad
- Re-encriptación disponible en el menú principal

**Archivo de Ejemplo:**
- Usa `connections.example.json` como plantilla
- Nunca incluye credenciales reales
- Perfecto para documentación y nuevos desarrolladores

## 🛡️ Seguridad

### Encriptación Avanzada
- **AES-256**: Algoritmo de encriptación robusto para contraseñas
- **Claves por máquina**: Cada máquina genera claves únicas basadas en hardware
- **Protección en reposo**: Las contraseñas nunca se almacenan en texto plano
- **Re-encriptación**: Funcionalidad para actualizar encriptación existente

### Protección de Repositorio
- **`.gitignore`**: Archivo de conexiones excluido del control de versiones
- **Separación de datos**: Configuraciones sensibles fuera del código fuente
- **Plantillas seguras**: Archivos de ejemplo sin credenciales reales

### Mejores Prácticas Implementadas
- Validación de entrada para prevenir inyección SQL
- Timeout configurables para evitar conexiones colgantes
- Manejo seguro de excepciones sin exposición de datos sensibles
- Logs sin información de credenciales

## 📊 Reportería

### Generación de Reportes Excel
- **Formato profesional**: Tablas con estilo corporativo
- **Estadísticas automáticas**: Resumen de conexiones exitosas/fallidas
- **Filtros avanzados**: Datos organizados por estado y rendimiento
- **Métricas detalladas**: Tiempos de respuesta y versiones de SQL Server
- **Exportación automática**: Archivos guardados con timestamp

### Información Incluida
- Estado de todas las conexiones configuradas
- Tiempos de respuesta y performance
- Versiones de SQL Server detectadas
- Estadísticas de éxito/error
- Marcas de tiempo de última prueba

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

**Error de encriptación**: No se pueden descifrar contraseñas
- Verifica que estés usando la misma máquina donde se encriptaron
- Usa la opción "Re-encriptar contraseñas" del menú
- En caso extremo, elimina y vuelve a crear las conexiones

**Archivo connections.json no encontrado**
- La aplicación creará el archivo automáticamente
- Usa `connections.example.json` como referencia
- Verifica permisos de escritura en la carpeta Data

## 🔄 Actualización y Compilación

Para actualizar las dependencias:
```bash
dotnet restore
```

Para compilar en modo debug:
```bash
dotnet build
```

Para compilar en modo release:
```bash
dotnet build --configuration Release
```

Para ejecutar la aplicación:
```bash
dotnet run
```

## 🏗️ Desarrollo

### Arquitectura del Proyecto
- **Separación de responsabilidades**: Cada servicio tiene una función específica
- **Inyección de dependencias**: Servicios loosely coupled
- **Patrones implementados**: Repository, Service Layer, Factory
- **Manejo de errores**: Try-catch comprehensivo con logging

### Extensibilidad
- Fácil agregar nuevos tipos de base de datos
- Servicios de reportería modulares
- Encriptación configurable
- Interfaz de usuario extensible

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