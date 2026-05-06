using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Services.Interfaces;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.Windows.Storage.Pickers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace FileSystemViewer.ViewModels
{
    public partial class ReportViewerPageViewModel : ViewModelBase
    {
        public ReportViewerPageViewModel(IServiceProvider serviceProvider, IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider, 
            IFileExtentionItemService fileExtentionItemService, IConfigurationService<AppSettings> configurationService, 
            IVisualManagerService visualManagerService, INotificationService notificationService, IReportStorageService reportStorageService, 
            IDialogService dialogService, AppState appState) : base(serviceProvider, driveUtilsService, dispatcherQueueProvider, 
                fileExtentionItemService, configurationService, visualManagerService, notificationService, reportStorageService, dialogService, appState)
        {
            ScanReports = new ObservableCollection<ScanReport>();
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                base.Dispose();
            }
        }

        public ObservableCollection<ScanReport> ScanReports { get; set; }

        [ObservableProperty]
        public partial ScanReport? SelectedTab { get; set; }

        [RelayCommand]
        public void CloseAllReports()
        {
            ScanReports.Clear();
            ApplicationState.ReportViewerVisibility = Visibility.Collapsed;
        }

        [RelayCommand]
        public void CloseIndividualReport(ScanReport report)
        {
            if (ScanReports.Contains(report))
            {
                ScanReports.Remove(report);
            }
            if (!ScanReports.Any())
            {
                ApplicationState.ReportViewerVisibility = Visibility.Collapsed;
            }
        }

        [RelayCommand]
        public async Task OpenReport()
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker(Win32Interop.GetWindowIdFromWindow(ApplicationState.MainWindowHandle))
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            fileOpenPicker.FileTypeFilter.Add(AppState.ReportFileExtension);
            var file = await fileOpenPicker.PickSingleFileAsync();

            if (file != null)
            {
                string jsonText = File.ReadAllText(file.Path);
                if (!string.IsNullOrWhiteSpace(jsonText))
                {
                    try
                    {
                        RetrieveAndOpenScanReport(file.Path, true);
                    }
                    catch (Exception ex)
                    {
                        await DialogService.ShowOpenReportErrorAsync(ex.Message);
                    }
                }
            }
        }

        public ScanReport? RetrieveAndOpenScanReport(string path, bool tryOpen)
        {
            string jsonText = File.ReadAllText(path);
            if (!string.IsNullOrWhiteSpace(jsonText))
            {
                ScanReport? selectedReport = JsonSerializer.Deserialize<ScanReport>(jsonText);

                if (tryOpen && selectedReport != null)
                {
                    ScanReports.Add(selectedReport);
                    SelectedTab = selectedReport;
                    ApplicationState.ReportViewerVisibility = Visibility.Visible;
                }
                return selectedReport;
            }
            return null;
        }

        public void OpenFileReports(List<IStorageItem> fileReports)
        {
            bool isAtLeastOneSuccess = false;
            var firstItem = fileReports.FirstOrDefault();
            foreach (StorageFile file in fileReports)
            {
                try
                {
                    if (file.FileType.Equals(AppState.ReportFileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        ScanReport? report = RetrieveAndOpenScanReport(file.Path, false);

                        if (report != null)
                        {
                            ScanReports.Add(report);

                            if (firstItem is StorageFile stf && stf.Equals(file))
                            {
                                SelectedTab = report;
                            }
                        }
                        isAtLeastOneSuccess = true;
                    } 
                }
                catch (Exception)
                {
                    continue;
                }
            }

            if (isAtLeastOneSuccess)
            {
                ApplicationState.ReportViewerVisibility = Visibility.Visible;
            }
        }
    }
}
