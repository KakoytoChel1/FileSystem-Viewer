using CommunityToolkit.Mvvm.ComponentModel;
using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Services.Interfaces;
using WinUI3Localizer;

namespace FileSystemViewer.ViewModels
{
    public abstract class ViewModelBase(IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider, 
        IFileExtentionItemService fileExtentionItemService, IConfigurationService<AppSettings> configurationService, 
        IVisualManagerService visualManagerService, AppState appState) : ObservableObject
    {
        public IDriveUtilsService DriveUtilsService { get; } = driveUtilsService;
        public IDispatcherQueueProvider DispatcherQueueProvider { get; } = dispatcherQueueProvider;
        public IFileExtentionItemService FileExtentionItemService { get; } = fileExtentionItemService;
        public IConfigurationService<AppSettings> ConfigurationService { get; } = configurationService;
        public IVisualManagerService VisualManagerService { get; } = visualManagerService;
        public AppState ApplicationState { get; } = appState;
        public ILocalizer Localizer => WinUI3Localizer.Localizer.Get();
    }
}
