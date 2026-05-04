using FileSystemViewer.Models.DataModels;
using System;
using System.IO;
using System.Text.Json;

namespace FileSystemViewer.ViewModels.Tools
{
    public static class ScanningReportHelper
    {
        public static string SaveReport(ScanReport scanReport)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string appFolder = Path.Combine(documentsPath, "FileSystemViewer");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            string fileName = $"ScanReport_{scanReport.ScanDateTime.ToString("yyyy-MM-dd_HH-mm-ss")}{AppState.ReportFileExtension}";
            scanReport.Name = fileName;

            string filePath = Path.Combine(appFolder, fileName);
            string json = JsonSerializer.Serialize(scanReport, options);
            File.WriteAllText(filePath, json);
            return filePath;
        }
    }
}
