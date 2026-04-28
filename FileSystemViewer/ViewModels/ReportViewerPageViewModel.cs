using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels.Tools;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
            IVisualManagerService visualManagerService, AppState appState) : base(serviceProvider, driveUtilsService, dispatcherQueueProvider, 
                fileExtentionItemService, configurationService, visualManagerService, appState)
        {
            ScanReports = new ObservableCollection<ScanReport>();
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
        public async Task OpenReport(XamlRoot xamlRoot)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker(Win32Interop.GetWindowIdFromWindow(ApplicationState.MainWindowHandle))
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            fileOpenPicker.FileTypeFilter.Add(".json");
            var file = await fileOpenPicker.PickSingleFileAsync();

            if (file != null)
            {
                string jsonText = File.ReadAllText(file.Path);
                if (!string.IsNullOrWhiteSpace(jsonText))
                {
                    try
                    {
                        ScanReport? selectedReport = JsonSerializer.Deserialize<ScanReport>(jsonText);
                        if (selectedReport != null)
                        {
                            ScanReports.Add(selectedReport);
                            SelectedTab = selectedReport;
                        }
                    }
                    catch (Exception ex)
                    {
                        await DialogManager.ShowContentDialogAsync(xamlRoot!, Localizer.GetLocalizedString("DialogErrorTitle"), Localizer.GetLocalizedString("DialogOkay"), ContentDialogButton.Primary, $"{Localizer.GetLocalizedString("DialogFailedImportSettingsText")} {ex.Message}");
                    }
                }
            }
        }

        public void OpenFileReports(List<IStorageItem> fileReports)
        {
            bool isAtLeastOneSuccess = false;
            var firstItem = fileReports.FirstOrDefault();
            foreach (StorageFile file in fileReports)
            {
                try
                {
                    if (file.FileType.Equals(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        string json = File.ReadAllText(file.Path);
                        ScanReport? report = JsonSerializer.Deserialize<ScanReport>(json);

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
