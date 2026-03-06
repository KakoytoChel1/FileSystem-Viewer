using CommunityToolkit.Mvvm.ComponentModel;
using FileSystem_Viewer.Services.IServices;

namespace FileSystem_Viewer.ViewModels
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
