using CommunityToolkit.Mvvm.Input;
using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Services.Interfaces;
using System;

namespace FileSystemViewer.ViewModels
{
    public partial class ChartPageViewModel : ViewModelBase
    {
        private ISubWindowManagerService _subWindowManagerService;

        public ChartPageViewModel(IServiceProvider serviceProvider, IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider, 
            IFileExtentionItemService fileExtentionItemService, IConfigurationService<AppSettings> configurationService, 
            IVisualManagerService visualManagerService, INotificationService notificationService, IReportStorageService reportStorageService,
            IDialogService dialogService, ISubWindowManagerService subWindowManagerService, AppState appState) : base(serviceProvider, driveUtilsService, dispatcherQueueProvider, fileExtentionItemService, 
                configurationService, visualManagerService, notificationService, reportStorageService, dialogService, appState)
        {
            _subWindowManagerService = subWindowManagerService;
        }

        [RelayCommand]
        public void OpenChartTabsNewWindow()
        {
            _subWindowManagerService.OpenExtensionChartSubWindow();
        }
    }
}
