using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Services;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels.Tools;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.Storage.Pickers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace FileSystemViewer.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private IBackgroundSchedulerService _backgroundSchedulerService;
        private readonly TimeSpan _minimumTime = TimeSpan.FromMinutes(15);

        public SettingsViewModel(IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider,
            IFileExtentionItemService fileExtentionItemService, IConfigurationService<AppSettings> configurationService, 
            IBackgroundSchedulerService backgroundSchedulerService, IVisualManagerService visualManagerService, AppState appState) : base(driveUtilsService, dispatcherQueueProvider,
                fileExtentionItemService, configurationService, visualManagerService, appState)
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
            MinCriticalFreeSpacePercent != ConfigurationService.Settings.MinFreeSpacePercent ||
            IsSystemAccentColorUsed != ConfigurationService.Settings.IsSystemAccentColorUsed ||
            GetSelectedTheme() != ConfigurationService.Settings.AppTheme ||
            GetSelectedLanguage() != ConfigurationService.Settings.AppLanguage ||
            CustomAccentColor != ConfigurationService.Settings.CustomAccentColor;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsThereUnsavedChanges))]
        public partial bool IsTrayToggleOn { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsThereUnsavedChanges))]
        public partial bool IsSchedulerScanToggleOn { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsThereUnsavedChanges))]
        public partial TimeSpan ScheduledScanTimeSpan { get; set; }
        partial void OnScheduledScanTimeSpanChanged(TimeSpan value)
        {
            if (value < _minimumTime)
            {
                ScheduledScanTimeSpan = _minimumTime;
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsThereUnsavedChanges))]
        public partial double MinCriticalFreeSpacePercent { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsThereUnsavedChanges))]
        public partial bool IsSystemAccentColorUsed { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsThereUnsavedChanges))]
        public partial bool IsDarkThemeSelected { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsThereUnsavedChanges))]
        public partial bool IsLightThemeSelected { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsThereUnsavedChanges))]
        public partial bool IsSystemThemeSelected { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsThereUnsavedChanges))]
        public partial bool IsUALanguageSelected { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsThereUnsavedChanges))]
        public partial bool IsENGLanguageSelected { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsThereUnsavedChanges))]
        public partial Color CustomAccentColor { get; set; }

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
                        await DialogManager.ShowContentDialogAsync(xamlRoot!, Localizer.GetLocalizedString("DialogErrorTitle"), Localizer.GetLocalizedString("DialogOkay"), ContentDialogButton.Primary, $"{Localizer.GetLocalizedString("DialogFailedImportSettingsText")} {ex.Message}");
                    }
                }
            }
        }

        [RelayCommand]
        public async Task ExportSettings(XamlRoot xamlRoot)
        {
            if (IsThereUnsavedChanges)
            {
                await DialogManager.ShowContentDialogAsync(xamlRoot!, Localizer.GetLocalizedString("DialogExportSettingsWarningTitle"), Localizer.GetLocalizedString("DialogOkay"), ContentDialogButton.Primary, Localizer.GetLocalizedString("DialogExportSettingsWarningText"));
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
            var result = await DialogManager.ShowContentDialogAsync(xamlRoot!, Localizer.GetLocalizedString("DialogSetSettingsByDefaultTitle"), Localizer.GetLocalizedString("DialogConfirmText"), 
                ContentDialogButton.Primary, Localizer.GetLocalizedString("DialogSetSettingsByDefaultText"), closeBtnText: Localizer.GetLocalizedString("DialogCancelText"));
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

            var result = await DialogManager.ShowContentDialogAsync(xamlRoot!, Localizer.GetLocalizedString("DialogRestoreSettingsTitle"), Localizer.GetLocalizedString("DialogConfirmText"), 
                ContentDialogButton.Primary, Localizer.GetLocalizedString("DialogRestoreSettingsText"), closeBtnText: Localizer.GetLocalizedString("DialogCancelText"));
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
                AppSettings.ThemeMode selectedTheme = GetSelectedTheme();
                AppSettings.Language selectedLanguage = GetSelectedLanguage();

                RescheduleBackgroundTask();
                RefreshVisualSettings(selectedTheme, selectedLanguage);
                SaveSettingsToConfig(selectedTheme, selectedLanguage);
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

            var result = await DialogManager.ShowContentDialogAsync(xamlRoot!, Localizer.GetLocalizedString("DialogUnsavedChangesTitle"), Localizer.GetLocalizedString("DialogConfirmText"), 
                ContentDialogButton.Primary, Localizer.GetLocalizedString("DialogUnsavedChangesText"), closeBtnText: Localizer.GetLocalizedString("DialogCancelText"));

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

            IsSystemAccentColorUsed = appSettings.IsSystemAccentColorUsed;

            var selectedTheme = appSettings.AppTheme;
            var selectedLanguage = appSettings.AppLanguage;

            IsDarkThemeSelected = selectedTheme == AppSettings.ThemeMode.Dark;
            IsLightThemeSelected = selectedTheme == AppSettings.ThemeMode.Light;
            IsSystemThemeSelected = selectedTheme == AppSettings.ThemeMode.System;
            
            CustomAccentColor = appSettings.CustomAccentColor;

            IsENGLanguageSelected = selectedLanguage == AppSettings.Language.English;
            IsUALanguageSelected = selectedLanguage == AppSettings.Language.Ukrainian;
        }

        private void SaveSettingsToConfig(AppSettings.ThemeMode selectedTheme, AppSettings.Language selectedLanguage)
        {
            ConfigurationService.Settings!.IsTrayActive = IsTrayToggleOn;
            ConfigurationService.Settings.IsScheduledScanningEnabled = IsSchedulerScanToggleOn;
            ConfigurationService.Settings.ScheduledScanningTime = ScheduledScanTimeSpan;
            ConfigurationService.Settings.MinFreeSpacePercent = MinCriticalFreeSpacePercent;

            ConfigurationService.Settings.AppTheme = selectedTheme;
            ConfigurationService.Settings.AppLanguage = selectedLanguage;
            ConfigurationService.Settings.IsSystemAccentColorUsed = IsSystemAccentColorUsed;
            ConfigurationService.Settings.CustomAccentColor = CustomAccentColor;

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
                _backgroundSchedulerService.UpdateIntervalTaskTimeAsync(ScheduledScanTimeSpan);
            }
            // If both is false, we do nothing
            else if (currentScheduleEnabledValue == false && newScheduleEnabledValue == currentScheduleEnabledValue)
            {
                return;
            }
            // If new value is true, we should create scheduler
            else if (currentScheduleEnabledValue == false && newScheduleEnabledValue == true)
            {
                _backgroundSchedulerService.RegisterIntervalTaskAsync(ScheduledScanTimeSpan);
            }
            // If new value is false, we should delete existing scheduler
            else if (currentScheduleEnabledValue == true && newScheduleEnabledValue == false)
            {
                _backgroundSchedulerService.DeleteIntervalTask(BackgroundSchedulerService.TaskName);
            }
        }

        private void RefreshVisualSettings(AppSettings.ThemeMode selectedTheme, AppSettings.Language selectedLanguage)
        {
            VisualManagerService.SetApplicationTheme(selectedTheme);
            
            if (selectedLanguage != ConfigurationService.Settings!.AppLanguage)
            {
                WinUI3Localizer.Localizer.Get().SetLanguage(selectedLanguage == AppSettings.Language.English ? "en-GB" : "uk-UA");
            }

            if (!IsSystemAccentColorUsed)
            {
                VisualManagerService.SetAccentColor(CustomAccentColor);
            }
            else
            {
                UISettings uiSettings = new();
                Color systemAccentColor = uiSettings.GetColorValue(UIColorType.Accent);
                VisualManagerService.SetAccentColor(systemAccentColor);
            }
        }

        private AppSettings.ThemeMode GetSelectedTheme()
        {
            if (IsDarkThemeSelected)
            {
                return AppSettings.ThemeMode.Dark;
            }
            else if (IsLightThemeSelected)
            {
                return AppSettings.ThemeMode.Light;
            }
            else
            {
                return AppSettings.ThemeMode.System;
            }
        }

        private AppSettings.Language GetSelectedLanguage()
        {
            if (IsENGLanguageSelected)
            {
                return AppSettings.Language.English;
            }
            else
            {
                return AppSettings.Language.Ukrainian;
            }
        }
    }
}
