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
        public Task<bool> ShowOpenReportErrorAsync(string exMessage);
        public Task<bool> ShowSettingsImportErrorAsync(string exMessage);
        public Task<bool> ShowUnsavedChangesWarningAsync();
        public Task<bool> ConfirmSettingsByDefaultAsync();
        public Task<bool> ConfirmRestoreSettingsAsync();
        public Task<bool> ConfirmClosingSettingsMenuAsync();
        public Task<bool> ShowUnhandledExceptionAsync(Exception exception);
    }
}
