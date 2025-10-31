using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using TestingSQLServerCnx.Models;

namespace TestingSQLServerCnx.Services;

public class ExcelReportService
{
    public string GenerateConnectionsReport(List<ConnectionConfiguration> connections)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        
        var fileName = $"Reporte_Conexiones_SQL_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reportes", fileName);
        
        // Crear directorio si no existe
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        
        using var package = new ExcelPackage();
        
        // Crear hoja principal
        var worksheet = package.Workbook.Worksheets.Add("Conexiones SQL Server");
        
        // Configurar encabezados
        SetupHeaders(worksheet);
        
        // Llenar datos
        FillConnectionData(worksheet, connections);
        
        // Aplicar formato
        ApplyFormatting(worksheet, connections.Count);
        
        // Crear hoja de estadísticas
        CreateStatsSheet(package, connections);
        
        // Guardar archivo
        package.SaveAs(filePath);
        
        return filePath;
    }
    
    private void SetupHeaders(ExcelWorksheet worksheet)
    {
        var headers = new[]
        {
            "ID",
            "Nombre Conexión",
            "Servidor",
            "Base de Datos",
            "Usuario",
            "Tipo Autenticación",
            "Estado Última Prueba",
            "Tiempo Respuesta (ms)",
            "Versión SQL Server",
            "Fecha Creación",
            "Última Prueba",
            "Mensaje Resultado",
            "Connection Timeout",
            "Command Timeout"
        };
        
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = headers[i];
        }
    }
    
    private void FillConnectionData(ExcelWorksheet worksheet, List<ConnectionConfiguration> connections)
    {
        for (int i = 0; i < connections.Count; i++)
        {
            var conn = connections[i];
            var row = i + 2; // +2 porque empezamos en fila 2 (fila 1 son headers)
            
            worksheet.Cells[row, 1].Value = conn.Id;
            worksheet.Cells[row, 2].Value = conn.Name;
            worksheet.Cells[row, 3].Value = conn.Server;
            worksheet.Cells[row, 4].Value = conn.Database;
            worksheet.Cells[row, 5].Value = conn.Username;
            worksheet.Cells[row, 6].Value = conn.IntegratedSecurity ? "Windows" : "SQL Server";
            
            if (conn.LastTestResult != null)
            {
                worksheet.Cells[row, 7].Value = conn.LastTestResult.IsSuccessful ? "EXITOSA" : "FALLÓ";
                worksheet.Cells[row, 8].Value = conn.LastTestResult.ResponseTime.TotalMilliseconds;
                worksheet.Cells[row, 9].Value = conn.LastTestResult.SqlServerVersion ?? "N/A";
                worksheet.Cells[row, 12].Value = conn.LastTestResult.Message;
                
                // Aplicar color según el resultado
                var resultCell = worksheet.Cells[row, 7];
                if (conn.LastTestResult.IsSuccessful)
                {
                    resultCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    resultCell.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                    resultCell.Style.Font.Color.SetColor(Color.DarkGreen);
                }
                else
                {
                    resultCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    resultCell.Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                    resultCell.Style.Font.Color.SetColor(Color.DarkRed);
                }
            }
            else
            {
                worksheet.Cells[row, 7].Value = "NO PROBADA";
                worksheet.Cells[row, 8].Value = 0;
                worksheet.Cells[row, 9].Value = "N/A";
                worksheet.Cells[row, 12].Value = "Sin pruebas realizadas";
            }
            
            worksheet.Cells[row, 10].Value = conn.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cells[row, 11].Value = conn.LastTested?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Nunca";
            worksheet.Cells[row, 13].Value = conn.ConnectionTimeout;
            worksheet.Cells[row, 14].Value = conn.CommandTimeout;
        }
    }
    
    private void ApplyFormatting(ExcelWorksheet worksheet, int dataRows)
    {
        var totalRows = dataRows + 1; // +1 por el header
        
        // Formato de encabezados
        using (var headerRange = worksheet.Cells[1, 1, 1, 14])
        {
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
            headerRange.Style.Font.Color.SetColor(Color.White);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }
        
        // Autoajustar columnas
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        
        // Aplicar bordes
        using (var dataRange = worksheet.Cells[1, 1, totalRows, 14])
        {
            dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }
        
        // Formato de fechas
        if (dataRows > 0)
        {
            worksheet.Cells[2, 10, totalRows, 11].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
        }
        
        // Formato de números (tiempo de respuesta)
        if (dataRows > 0)
        {
            worksheet.Cells[2, 8, totalRows, 8].Style.Numberformat.Format = "#,##0.00";
        }
        
        // Congelar la primera fila
        worksheet.View.FreezePanes(2, 1);
    }
    
    private void CreateStatsSheet(ExcelPackage package, List<ConnectionConfiguration> connections)
    {
        var statsSheet = package.Workbook.Worksheets.Add("Estadísticas");
        
        // Título
        statsSheet.Cells[1, 1].Value = "ESTADÍSTICAS DE CONEXIONES SQL SERVER";
        statsSheet.Cells[1, 1].Style.Font.Size = 16;
        statsSheet.Cells[1, 1].Style.Font.Bold = true;
        
        // Fecha del reporte
        statsSheet.Cells[2, 1].Value = $"Generado el: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        
        var row = 4;
        
        // Total de conexiones
        statsSheet.Cells[row, 1].Value = "Total de Conexiones:";
        statsSheet.Cells[row, 2].Value = connections.Count;
        row++;
        
        // Conexiones por tipo de autenticación
        var sqlAuthCount = connections.Count(c => !c.IntegratedSecurity);
        var winAuthCount = connections.Count(c => c.IntegratedSecurity);
        
        statsSheet.Cells[row, 1].Value = "Autenticación SQL Server:";
        statsSheet.Cells[row, 2].Value = sqlAuthCount;
        row++;
        
        statsSheet.Cells[row, 1].Value = "Autenticación Windows:";
        statsSheet.Cells[row, 2].Value = winAuthCount;
        row += 2;
        
        // Estadísticas de pruebas
        var testedConnections = connections.Where(c => c.LastTestResult != null).ToList();
        var successfulTests = testedConnections.Count(c => c.LastTestResult!.IsSuccessful);
        var failedTests = testedConnections.Count(c => !c.LastTestResult!.IsSuccessful);
        
        statsSheet.Cells[row, 1].Value = "RESULTADOS DE PRUEBAS";
        statsSheet.Cells[row, 1].Style.Font.Bold = true;
        row++;
        
        statsSheet.Cells[row, 1].Value = "Conexiones Probadas:";
        statsSheet.Cells[row, 2].Value = testedConnections.Count;
        row++;
        
        statsSheet.Cells[row, 1].Value = "Pruebas Exitosas:";
        statsSheet.Cells[row, 2].Value = successfulTests;
        statsSheet.Cells[row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
        statsSheet.Cells[row, 2].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
        row++;
        
        statsSheet.Cells[row, 1].Value = "Pruebas Fallidas:";
        statsSheet.Cells[row, 2].Value = failedTests;
        statsSheet.Cells[row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
        statsSheet.Cells[row, 2].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
        row++;
        
        if (testedConnections.Count > 0)
        {
            statsSheet.Cells[row, 1].Value = "Tasa de Éxito:";
            var successRate = (double)successfulTests / testedConnections.Count * 100;
            statsSheet.Cells[row, 2].Value = $"{successRate:F1}%";
            row += 2;
            
            // Tiempo promedio de respuesta
            var avgResponseTime = testedConnections
                .Where(c => c.LastTestResult!.IsSuccessful)
                .Average(c => c.LastTestResult!.ResponseTime.TotalMilliseconds);
            
            statsSheet.Cells[row, 1].Value = "Tiempo Promedio Respuesta:";
            statsSheet.Cells[row, 2].Value = $"{avgResponseTime:F2} ms";
        }
        
        // Autoajustar columnas
        statsSheet.Cells[statsSheet.Dimension.Address].AutoFitColumns();
        
        // Aplicar formato
        using (var titleRange = statsSheet.Cells[1, 1, 1, 2])
        {
            titleRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            titleRange.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
            titleRange.Style.Font.Color.SetColor(Color.White);
        }
    }
}