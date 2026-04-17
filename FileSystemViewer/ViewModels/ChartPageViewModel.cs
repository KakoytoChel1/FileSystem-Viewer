using CommunityToolkit.Mvvm.Input;
using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.Views.Windows;

namespace FileSystemViewer.ViewModels
{
    public partial class ChartPageViewModel : ViewModelBase
    {
        public ChartPageViewModel(IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider, 
            IFileExtentionItemService fileExtentionItemService, IConfigurationService<AppSettings> configurationService, 
            IVisualManagerService visualManagerService, AppState appState) : base(driveUtilsService, dispatcherQueueProvider, fileExtentionItemService, configurationService, visualManagerService, appState) { }

        [RelayCommand]
        public void OpenChartTabsNewWindow()
        {
            string windowKey = nameof(ChartTabsWindow);

            if (!ApplicationState.ActiveSubWindows.ContainsKey(windowKey))
            {
                ChartTabsWindow chartTabsWindow = new ChartTabsWindow();
                chartTabsWindow.Closed += (s, e) => ApplicationState.ActiveSubWindows.Remove(windowKey);
                ApplicationState.ActiveSubWindows.Add(windowKey, chartTabsWindow);
                chartTabsWindow.Activate();
            }
        }
    }
}
