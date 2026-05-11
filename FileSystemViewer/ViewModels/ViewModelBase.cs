using CommunityToolkit.Mvvm.ComponentModel;
using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Services.Interfaces;
using System;
using WinUI3Localizer;

namespace FileSystemViewer.ViewModels
{
    public abstract class ViewModelBase(IServiceProvider serviceProvider, IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider, 
        IFileExtentionItemService fileExtentionItemService, IConfigurationService<AppSettings> configurationService, 
        IVisualManagerService visualManagerService, INotificationService notificationService, IReportStorageService reportStorageService, 
        IDialogService dialogService, AppState appState) : ObservableObject, IDisposable
    {
        protected bool _disposed = false;
        protected IServiceProvider? ServiceProvider { get; private set; } = serviceProvider;
        public IDriveUtilsService DriveUtilsService { get; } = driveUtilsService;
        public IDispatcherQueueProvider DispatcherQueueProvider { get; } = dispatcherQueueProvider;
        public IFileExtentionItemService FileExtentionItemService { get; } = fileExtentionItemService;
        public IConfigurationService<AppSettings> ConfigurationService { get; } = configurationService;
        public IVisualManagerService VisualManagerService { get; } = visualManagerService;
        public INotificationService NotificationService { get; } = notificationService;
        public IReportStorageService ReportStorageService { get; } = reportStorageService;
        public IDialogService DialogService { get; } = dialogService;
        public AppState ApplicationState { get; } = appState;
        public ILocalizer Localizer => WinUI3Localizer.Localizer.Get();

        public virtual void Dispose()
        {
            _disposed = true;
            ServiceProvider = null;
        }
    }
}
