using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;

namespace FileSystemViewer.Services.Interfaces
{
    public interface IDialogService
    {
        public void Initialize(XamlRoot xamlRoot);
        public Task<bool> ShowTargetSelectionDialogAsync(IServiceProvider serviceProvider);
        public Task<bool> ConfirmRefreshScanningAsync(int nodesCount);
        public Task<bool> ConfirmSelectedDirectoryScanningAsync();
        public Task<bool> ConfirmScanningCancellingAsync();
        public Task<bool> ShowOpenReportErrorAsync();
        public Task<bool> ShowSettingsImportErrorAsync();
        public Task<bool> ShowUnsavedChangesWarningAsync();
        public Task<bool> ConfirmSettingsByDefaultAsync();
        public Task<bool> ConfirmRestoreSettingsAsync();
        public Task<bool> ShowUnhandledExceptionAsync(Exception exception);
    }
}
