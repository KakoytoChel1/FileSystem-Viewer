using System;
using System.IO;

namespace FileSystemViewer.Models.Tools
{
    public static class Logger
    {
        private static string? _logFilePath;
        private static readonly object _lockObj = new object();

        public static void Log(string message)
        {
            lock (_lockObj)
            {
                try
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    _logFilePath = Path.Combine(baseDirectory, "app_log.txt");

                    string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                    File.AppendAllText(_logFilePath, logEntry);
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
