using FileSystemViewer.Services.Interfaces;
using System;
using System.IO;
using System.Text.Json;

namespace FileSystemViewer.Services
{
    public class ConfigurationService<T> : IConfigurationService where T : class, new()
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public T Settings { get; private set; } = null!;

        public ConfigurationService(string fileName = "appSettings.json")
        {
            _filePath = Path.Combine(AppContext.BaseDirectory, fileName);

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            Load();
        }

        public void Load()
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    string json = File.ReadAllText(_filePath);
                    Settings = JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? new T();
                }
                catch (Exception)
                {
                    Settings = new T();
                }
            }
            else
            {
                Settings = new T();
                Save();
            }
        }

        public void Save()
        {
            string json = JsonSerializer.Serialize(Settings, _jsonOptions);
            File.WriteAllText(_filePath, json);
        }
    }
}
