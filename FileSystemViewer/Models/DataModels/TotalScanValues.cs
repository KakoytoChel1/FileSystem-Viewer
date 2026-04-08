namespace FileSystemViewer.Models.DataModels
{
    public class TotalScanValues
    {
        public TotalScanValues() { }

        public long TotalFileCount { get; set; }
        public long TotalSizeInBytes { get; set; }
        public long TotalDirectoryCount { get; set; }
    }
}
