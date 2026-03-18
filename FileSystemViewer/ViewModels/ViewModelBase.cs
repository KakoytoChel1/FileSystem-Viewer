using CommunityToolkit.Mvvm.ComponentModel;
using FileSystemViewer.Services.Interfaces;

namespace FileSystemViewer.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {
        public IDriveUtilsService DriveUtilsService { get; }
        public IDispatcherQueueProvider DispatcherQueueProvider { get; }

        public ViewModelBase(IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider)
        {
            DriveUtilsService = driveUtilsService;
            DispatcherQueueProvider = dispatcherQueueProvider;
        }
    }
}
