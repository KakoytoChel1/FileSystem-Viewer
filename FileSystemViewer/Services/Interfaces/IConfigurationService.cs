namespace FileSystemViewer.Services.Interfaces
{
    public interface IConfigurationService<T>
    {
        public T Settings { get; }
        public void Load();
        public void Save();
    }
}
