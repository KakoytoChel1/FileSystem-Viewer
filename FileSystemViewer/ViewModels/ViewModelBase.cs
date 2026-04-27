using CommunityToolkit.Mvvm.ComponentModel;
using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Services.Interfaces;
using System;
using WinUI3Localizer;

namespace FileSystemViewer.ViewModels
{
    public abstract class ViewModelBase(IServiceProvider serviceProvider, IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider, 
        IFileExtentionItemService fileExtentionItemService, IConfigurationService<AppSettings> configurationService, 
        IVisualManagerService visualManagerService, AppState appState) : ObservableObject
    {
        protected IServiceProvider ServiceProvider { get; } = serviceProvider;
        public IDriveUtilsService DriveUtilsService { get; } = driveUtilsService;
        public IDispatcherQueueProvider DispatcherQueueProvider { get; } = dispatcherQueueProvider;
        public IFileExtentionItemService FileExtentionItemService { get; } = fileExtentionItemService;
        public IConfigurationService<AppSettings> ConfigurationService { get; } = configurationService;
        public IVisualManagerService VisualManagerService { get; } = visualManagerService;
        public AppState ApplicationState { get; } = appState;
        public ILocalizer Localizer => WinUI3Localizer.Localizer.Get();
    }
}
