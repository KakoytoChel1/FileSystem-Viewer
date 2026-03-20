using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels;

namespace FileSystem_Viewer.ViewModels
{
    public class ChartPageViewModel : ViewModelBase
    {
        public ChartPageViewModel(IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider, AppState appState) : base(driveUtilsService, dispatcherQueueProvider, appState)
        {
        }
    }
}
