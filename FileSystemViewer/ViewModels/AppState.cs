using CommunityToolkit.Mvvm.ComponentModel;
using FileSystemViewer.Models.DataModels;
using LiveChartsCore;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FileSystemViewer.ViewModels
{
    public partial class AppState : ObservableObject
    {
        public AppState()
        {
            FileExtensionItems = new ObservableCollection<FileExtensionItem>();
            FileExtensionSeriesCollection = new ObservableCollection<ISeries>();
            ScannedRootNodeNames = new ObservableCollection<string>();
            ActiveSubWindows = new Dictionary<string, Window>();
        }
        public enum ScanningStates
        {
            None,
            Completed,
            Paused,
            Canceled,
            InProgress
        }

        public event Action? ScanningStatePropertyChanged;

        private ScanningStates _currentScanningState;
        public ScanningStates CurrentScanningState
        {
            get { return _currentScanningState; }
            set
            {
                if (SetProperty(ref _currentScanningState, value))
                {
                    ScanningStatePropertyChanged?.Invoke();
                }
            }
        }

        public ObservableCollection<FileExtensionItem> FileExtensionItems { get; set; }
        public ObservableCollection<ISeries> FileExtensionSeriesCollection { get; set; }
        public ObservableCollection<string> ScannedRootNodeNames { get; set; }
        public Dictionary<string, Window> ActiveSubWindows { get; set; }

        public long TotalFilesScanned { get; set; }
        public long TotalDirectoriesScanned { get; set; }

        [ObservableProperty]
        public partial Visibility SettingsMenuVisibility { get; set; }

        [ObservableProperty]
        public partial Visibility ReportViewerVisibility { get; set; }

        public nint MainWindowHandle { get; private set; } = 0;

        public void SetMainWindowHandle(nint handle)
        {
            if (MainWindowHandle == 0)
            {
                MainWindowHandle = handle;
            }
        }
    }
}
