using CommunityToolkit.Mvvm.ComponentModel;
using FileSystem_Viewer.Models.DataModels;
using LiveChartsCore;
using System;
using System.Collections.ObjectModel;

namespace FileSystem_Viewer.ViewModels
{
    public class AppState : ObservableObject
    {
        public AppState()
        {
            FileExtensionItems = new ObservableCollection<FileExtensionItem>();
            FileExtensionSeriesCollection = new ObservableCollection<ISeries>();
            ScannedRootNodeNames = new ObservableCollection<string>();
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

        public long TotalFilesScanned { get; set; }
        public long TotalDirectoriesScanned { get; set; }
    }
}
