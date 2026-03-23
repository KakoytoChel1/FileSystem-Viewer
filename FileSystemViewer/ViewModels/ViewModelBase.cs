using CommunityToolkit.Mvvm.ComponentModel;
using FileSystem_Viewer.ViewModels;
using FileSystemViewer.Services.Interfaces;

namespace FileSystemViewer.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {
        public IDriveUtilsService DriveUtilsService { get; }
        public IDispatcherQueueProvider DispatcherQueueProvider { get; }
        public IFileExtentionItemService FileExtentionItemService { get; }
        public AppState ApplicationState { get; }

        public ViewModelBase(IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider, IFileExtentionItemService fileExtentionItemService, AppState appState)
        {
            DriveUtilsService = driveUtilsService;
            DispatcherQueueProvider = dispatcherQueueProvider;
            FileExtentionItemService = fileExtentionItemService;
            ApplicationState = appState;
        }
    }
}
