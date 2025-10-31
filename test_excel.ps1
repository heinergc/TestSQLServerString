# Script de prueba para el reporte Excel
echo "Probando la generación de reporte Excel..."

# Navegar al directorio del proyecto
cd E:\TestingSLQServerCnx

# Ejecutar la aplicación con la opción 10 (reporte Excel)
# Nota: En uso real, selecciona la opción 10 en el menú interactivo
dotnet run --project TestingSQLServerCnx.csproj

echo "Reporte completado. Revisa la carpeta Reportes en el directorio de salida."