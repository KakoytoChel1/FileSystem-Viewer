using CommunityToolkit.Mvvm.Input;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels.Tools;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace FileSystemViewer.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel(IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider, 
            IFileExtentionItemService fileExtentionItemService, AppState appState) : base(driveUtilsService, dispatcherQueueProvider, 
                fileExtentionItemService, appState)
        {
            ApplicationState.SettingsMenuVisibility = Visibility.Collapsed;
            MinCriticalFreeSpacePercent = 10;
        }

        private bool _isThereUnsavedChanges;
        public bool IsThereUnsavedChanges
        {
            get { return _isThereUnsavedChanges; }
            set { SetProperty(ref _isThereUnsavedChanges, value); }
        }

        private bool _isTrayToggleOn;
        public bool IsTrayToggleOn
        {
            get { return _isTrayToggleOn; }
            set { SetProperty(ref _isTrayToggleOn, value); }
        }

        private bool _isSchedulerScanToggleOn;
        public bool IsSchedulerScanToggleOn
        {
            get { return _isSchedulerScanToggleOn; }
            set { SetProperty(ref _isSchedulerScanToggleOn, value); }
        }

        private TimeSpan _scheduledScanTimeSpan;
        public TimeSpan ScheduledScanTimeSpan
        {
            get { return _scheduledScanTimeSpan; }
            set { SetProperty(ref _scheduledScanTimeSpan, value); }
        }

        private double _minCriticalFreeSpacePercent;
        public double MinCriticalFreeSpacePercent
        {
            get { return _minCriticalFreeSpacePercent; }
            set { SetProperty(ref _minCriticalFreeSpacePercent, value); }
        }

        private RelayCommand? _importSettingsCommand;
        public RelayCommand ImportSettingsCommand => _importSettingsCommand ??= new RelayCommand(() =>
        {
            
        });

        private RelayCommand? _exportSettingsCommand;
        public RelayCommand ExportSettingsCommand => _exportSettingsCommand ??= new RelayCommand(() =>
        {

        });

        private RelayCommand<XamlRoot>? _setSettingsByDefaultCommand;
        public RelayCommand<XamlRoot> SetSettingsByDefaultCommand => _setSettingsByDefaultCommand ??= new RelayCommand<XamlRoot>(async (xamlRoot) =>
        {
            var result = await DialogManager.ShowContentDialogAsync(xamlRoot!, "Set settings by default", "Confirm", ContentDialogButton.Primary, "Are you sure you want to set all settings by default?", closeBtnText: "Cancel");
            if (result == ContentDialogResult.Primary)
            {
                
            }
        });

        private RelayCommand? _saveSettingsCommand;
        public RelayCommand SaveSettingsCommand => _saveSettingsCommand ??= new RelayCommand(() =>
        {

        });

        private RelayCommand? _closeSettingsMenuCommand;
        public RelayCommand CloseSettingsMenuCommand => _closeSettingsMenuCommand ??= new RelayCommand(() =>
        {
            ApplicationState.SettingsMenuVisibility = Visibility.Collapsed;
        });
    }
}
