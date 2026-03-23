using CommunityToolkit.Mvvm.ComponentModel;
using FileSystem_Viewer.Models.DataModels;
using LiveChartsCore;
using System.Collections.ObjectModel;

namespace FileSystem_Viewer.ViewModels
{
    public class AppState : ObservableObject
    {
        public AppState()
        {
            FileExtensionItems = new ObservableCollection<FileExtensionItem>();
            FileExtensionSeriesCollection = new ObservableCollection<ISeries>();
        }

        public ObservableCollection<FileExtensionItem> FileExtensionItems { get; set; }
        public ObservableCollection<ISeries> FileExtensionSeriesCollection { get; set; }
    }
}
