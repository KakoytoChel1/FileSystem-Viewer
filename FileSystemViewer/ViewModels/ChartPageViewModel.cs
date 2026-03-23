using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels;
using LiveChartsCore;
using System.Collections.ObjectModel;

namespace FileSystem_Viewer.ViewModels
{
    public class ChartPageViewModel : ViewModelBase
    {
        public ChartPageViewModel(IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider, IFileExtentionItemService fileExtentionItemService, AppState appState) : base(driveUtilsService, dispatcherQueueProvider, fileExtentionItemService, appState)
        {
            
        }

        
    }
}
