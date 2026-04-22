using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Services.Interfaces;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace FileSystemViewer.ViewModels
{
    public class ReportViewerPageViewModel : ViewModelBase
    {
        public ReportViewerPageViewModel(IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider, IFileExtentionItemService fileExtentionItemService, IConfigurationService<AppSettings> configurationService, IVisualManagerService visualManagerService, AppState appState) : base(driveUtilsService, dispatcherQueueProvider, fileExtentionItemService, configurationService, visualManagerService, appState)
        {
            string json = File.ReadAllText(@"C:\Users\Vanya\Documents\FileSystemViewer\ScanReport_2026-04-22_19-08-53.json");
            ScanReport scanReport = JsonSerializer.Deserialize<ScanReport>(json);

            ScanReports = new ObservableCollection<ScanReport>();
            ScanReports.Add(scanReport);
        }

        public ObservableCollection<ScanReport> ScanReports { get; set; }
    }
}
