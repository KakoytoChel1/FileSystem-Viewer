using CommunityToolkit.Mvvm.ComponentModel;
using FileSystem_Viewer.ViewModels;
using FileSystemViewer.Services.Interfaces;

namespace FileSystemViewer.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {
        public IDriveUtilsService DriveUtilsService { get; }
        public IDispatcherQueueProvider DispatcherQueueProvider { get; }
        public AppState ApplicationState { get; }

        public ViewModelBase(IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider, AppState appState)
        {
            DriveUtilsService = driveUtilsService;
            DispatcherQueueProvider = dispatcherQueueProvider;
            ApplicationState = appState;
        }
    }
}
