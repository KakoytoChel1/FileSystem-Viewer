using CommunityToolkit.Mvvm.ComponentModel;
using FileSystemViewer.Services.Interfaces;

namespace FileSystemViewer.ViewModels
{
    public abstract class ViewModelBase(IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider, 
        IFileExtentionItemService fileExtentionItemService, AppState appState) : ObservableObject
    {
        public IDriveUtilsService DriveUtilsService { get; } = driveUtilsService;
        public IDispatcherQueueProvider DispatcherQueueProvider { get; } = dispatcherQueueProvider;
        public IFileExtentionItemService FileExtentionItemService { get; } = fileExtentionItemService;
        public AppState ApplicationState { get; } = appState;
    }
}
