using FileSystemViewer.Models.DataModels;
using System;

namespace FileSystemViewer.Services.Interfaces
{
    public interface IConfigurationService<T>
    {
        public event Action<T>? OnConfigurationChanged;
        public T Settings { get; }
        public void Load();
        public void Save();
    }
}
