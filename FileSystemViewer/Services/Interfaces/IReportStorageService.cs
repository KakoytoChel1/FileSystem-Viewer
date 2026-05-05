using FileSystemViewer.Models.DataModels;

namespace FileSystemViewer.Services.Interfaces
{
    public interface IReportStorageService
    {
        public string SaveReport(ScanReport report);
    }
}
