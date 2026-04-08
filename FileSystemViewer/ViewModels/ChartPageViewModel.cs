using CommunityToolkit.Mvvm.Input;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.Views.Windows;

namespace FileSystemViewer.ViewModels
{
    public class ChartPageViewModel : ViewModelBase
    {
        public ChartPageViewModel(IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider, IFileExtentionItemService fileExtentionItemService, AppState appState) : base(driveUtilsService, dispatcherQueueProvider, fileExtentionItemService, appState) { }

        private RelayCommand? _openChartTabsWindowCommand;
        public RelayCommand OpenChartTabsNewWindowCommand => _openChartTabsWindowCommand ??= new RelayCommand(async () =>
        {
            string windowKey = nameof(ChartTabsWindow);

            if (!ApplicationState.ActiveSubWindows.ContainsKey(windowKey))
            {
                ChartTabsWindow chartTabsWindow = new ChartTabsWindow();
                chartTabsWindow.Closed += (s, e) => ApplicationState.ActiveSubWindows.Remove(windowKey);
                ApplicationState.ActiveSubWindows.Add(windowKey, chartTabsWindow);
                chartTabsWindow.Activate();
            }
        });
    }
}
