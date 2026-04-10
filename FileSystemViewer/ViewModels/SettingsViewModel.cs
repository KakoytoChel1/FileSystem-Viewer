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
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace FileSystemViewer.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private IBackgroundSchedulerService _backgroundSchedulerService;

        public SettingsViewModel(IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider, 
            IFileExtentionItemService fileExtentionItemService, IConfigurationService<AppSettings> configurationService, IBackgroundSchedulerService backgroundSchedulerService, AppState appState) : base(driveUtilsService, dispatcherQueueProvider, 
                fileExtentionItemService, configurationService, appState)
        {
            ApplicationState.SettingsMenuVisibility = Visibility.Collapsed;
            _backgroundSchedulerService = backgroundSchedulerService;
            
            if (ConfigurationService.Settings == null)
            {
                ConfigurationService.Load();
            }
            RestoreSettingsPropertiesFrom(ConfigurationService.Settings!);
        }

        public bool IsThereUnsavedChanges =>
            IsTrayToggleOn != ConfigurationService.Settings!.IsTrayActive ||
            IsSchedulerScanToggleOn != ConfigurationService.Settings.IsScheduledScanningEnabled ||
            ScheduledScanTimeSpan != ConfigurationService.Settings.ScheduledScanningTime ||
            MinCriticalFreeSpacePercent != ConfigurationService.Settings.MinFreeSpacePercent;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsThereUnsavedChanges))]
        public partial bool IsTrayToggleOn { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsThereUnsavedChanges))]
        public partial bool IsSchedulerScanToggleOn { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsThereUnsavedChanges))]
        public partial TimeSpan ScheduledScanTimeSpan { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsThereUnsavedChanges))]
        public partial double MinCriticalFreeSpacePercent { get; set; }

        [RelayCommand]
        public async Task ImportSettings(XamlRoot xamlRoot)
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
                        AppSettings? importedSettings = JsonSerializer.Deserialize<AppSettings>(jsonText);
                        if (importedSettings != null)
                        {
                            RestoreSettingsPropertiesFrom(importedSettings);
                        }
                    }
                    catch (Exception ex)
                    {
                        await DialogManager.ShowContentDialogAsync(xamlRoot!, "Error", "Okay", ContentDialogButton.Primary, $"Failed to import settings: {ex.Message}");
                    }
                }
            }
        }

        [RelayCommand]
        public async Task ExportSettings(XamlRoot xamlRoot)
        {
            if (IsThereUnsavedChanges)
            {
                await DialogManager.ShowContentDialogAsync(xamlRoot!, "Unsaved changes", "Okay", ContentDialogButton.Primary, "Please save or reset your changes before exporting.");
                return;
            }

            FileSavePicker fileSavePicker = new FileSavePicker(Win32Interop.GetWindowIdFromWindow(ApplicationState.MainWindowHandle))
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                DefaultFileExtension = ".json",
                SuggestedFileName = "FileSystemViewer_Settings"
            };
            fileSavePicker.FileTypeChoices.Add("json config", new List<string>() { ".json" });
            var file = await fileSavePicker.PickSaveFileAsync();

            if (file != null)
            {
                string jsonText = JsonSerializer.Serialize(ConfigurationService.Settings!, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(file.Path, jsonText);
            }
        }

        [RelayCommand]
        public async Task SetSettingsByDefault(XamlRoot xamlRoot)
        {
            var result = await DialogManager.ShowContentDialogAsync(xamlRoot!, "Set settings by default", "Confirm", ContentDialogButton.Primary, "Are you sure you want to set all settings by default?", closeBtnText: "Cancel");
            if (result == ContentDialogResult.Primary)
            {
                RestoreSettingsPropertiesFrom(new AppSettings());
            }
        }

        [RelayCommand]
        public async Task RestoreSettings(XamlRoot xamlRoot)
        {
            if (!IsThereUnsavedChanges)
            {
                return;
            }

            var result = await DialogManager.ShowContentDialogAsync(xamlRoot!, "Restore settings", "Confirm", ContentDialogButton.Primary, "Are you sure you want to restore settings?", closeBtnText: "Cancel");
            if (result == ContentDialogResult.Primary)
            {
                RestoreSettingsPropertiesFrom(ConfigurationService.Settings!);
            }
        }

        [RelayCommand]
        public void SaveSettings()
        {
            if (IsThereUnsavedChanges)
            {
                RescheduleBackgroundTask();
                SaveSettingsToConfig();
            }
        }

        [RelayCommand]
        public async Task CloseSettingsMenu(XamlRoot xamlRoot)
        {
            if (!IsThereUnsavedChanges)
            {
                ApplicationState.SettingsMenuVisibility = Visibility.Collapsed;
                return;
            }

            var result = await DialogManager.ShowContentDialogAsync(xamlRoot!, "Unsaved changes", "Confirm", ContentDialogButton.Primary, "You have unsaved changes. Are you sure you want to close the settings menu?", closeBtnText: "Cancel");

            if (result == ContentDialogResult.Primary)
            {
                RestoreSettingsPropertiesFrom(ConfigurationService.Settings!);
                ApplicationState.SettingsMenuVisibility = Visibility.Collapsed;
            }
        }

        private void RestoreSettingsPropertiesFrom(AppSettings appSettings)
        {
            IsTrayToggleOn = appSettings.IsTrayActive;
            IsSchedulerScanToggleOn = appSettings.IsScheduledScanningEnabled;
            ScheduledScanTimeSpan = appSettings.ScheduledScanningTime;
            MinCriticalFreeSpacePercent = appSettings.MinFreeSpacePercent;
        }

        private void SaveSettingsToConfig()
        {
            ConfigurationService.Settings!.IsTrayActive = IsTrayToggleOn;
            ConfigurationService.Settings.IsScheduledScanningEnabled = IsSchedulerScanToggleOn;
            ConfigurationService.Settings.ScheduledScanningTime = ScheduledScanTimeSpan;
            ConfigurationService.Settings.MinFreeSpacePercent = MinCriticalFreeSpacePercent;
            ConfigurationService.Save();
            OnPropertyChanged(nameof(IsThereUnsavedChanges));
        }

        private void RescheduleBackgroundTask()
        {
            bool newScheduleEnabledValue = IsSchedulerScanToggleOn;
            bool currentScheduleEnabledValue = ConfigurationService.Settings!.IsScheduledScanningEnabled;

            // If both is true, we shoud update scheduler details
            if (currentScheduleEnabledValue == true && newScheduleEnabledValue == currentScheduleEnabledValue)
            {
                _backgroundSchedulerService.UpdateDailyTaskTime(ConfigurationService.Settings.ScheduledScanningTaskPath, ScheduledScanTimeSpan);
            }
            // If both is false, we do nothing
            else if (currentScheduleEnabledValue == false && newScheduleEnabledValue == currentScheduleEnabledValue)
            {
                return;
            }
            // If new value is true, we should create scheduler
            else if (currentScheduleEnabledValue == false && newScheduleEnabledValue == true)
            {
                _backgroundSchedulerService.RegisterDailyTask(ScheduledScanTimeSpan);
            }
            // If new value is false, we should delete existing scheduler
            else if (currentScheduleEnabledValue == true && newScheduleEnabledValue == false)
            {
                _backgroundSchedulerService.DeleteDailyTask(ConfigurationService.Settings.ScheduledScanningTaskPath);
            }
        }
    }
}
