using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.Views.DialogPages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using WinUI3Localizer;

namespace FileSystemViewer.Services
{
    public class DialogService : IDialogService
    {
        public ILocalizer _localizer => Localizer.Get();
        private XamlRoot? _xamlRoot;

        public void Initialize(XamlRoot xamlRoot)
        {
            _xamlRoot = xamlRoot;
        }

        public async Task<bool> ConfirmRefreshScanningAsync(int nodesCount)
        {
            if (_xamlRoot == null)
                ThrowXamlRootException();

            var dialogResult = await ShowContentDialogAsync(_xamlRoot!, _localizer.GetLocalizedString("DialogRefreshScanningTitle"), _localizer.GetLocalizedString("DialogConfirmText"),
               ContentDialogButton.Primary, $"{_localizer.GetLocalizedString("DialogRefreshScanningText")} {nodesCount}?", _localizer.GetLocalizedString("DialogCancelText"), null);
            
            return dialogResult == ContentDialogResult.Primary;
        }

        public async Task<bool> ConfirmRestoreSettingsAsync()
        {
            if (_xamlRoot == null)
                ThrowXamlRootException();

            var dialogResult = await ShowContentDialogAsync(_xamlRoot!, _localizer.GetLocalizedString("DialogRestoreSettingsTitle"), _localizer.GetLocalizedString("DialogConfirmText"),
                ContentDialogButton.Primary, _localizer.GetLocalizedString("DialogRestoreSettingsText"), closeBtnText: _localizer.GetLocalizedString("DialogCancelText"));

            return dialogResult == ContentDialogResult.Primary;
        }

        public async Task<bool> ConfirmScanningCancellingAsync()
        {
            if (_xamlRoot == null)
                ThrowXamlRootException();

            var dialogResult = await ShowContentDialogAsync(_xamlRoot!, _localizer.GetLocalizedString("DialogCancelScanningTitle"), _localizer.GetLocalizedString("DialogConfirmText"),
                ContentDialogButton.Primary, _localizer.GetLocalizedString("DialogCancelScanningText"), _localizer.GetLocalizedString("DialogCancelText"), null);

            return dialogResult == ContentDialogResult.Primary;
        }

        public async Task<bool> ConfirmSelectedDirectoryScanningAsync()
        {
            if (_xamlRoot == null)
                ThrowXamlRootException();

            var dialogResult = await ShowContentDialogAsync(_xamlRoot!, _localizer.GetLocalizedString("DialogRefreshSelectedTitle"), _localizer.GetLocalizedString("DialogConfirmText"),
                   ContentDialogButton.Primary, $"{_localizer.GetLocalizedString("DialogRefreshSelectedText")}", _localizer.GetLocalizedString("DialogCancelText"), null);

            return dialogResult == ContentDialogResult.Primary;
        }

        public async Task<bool> ConfirmSettingsByDefaultAsync()
        {
            if (_xamlRoot == null)
                ThrowXamlRootException();

            var dialogResult = await ShowContentDialogAsync(_xamlRoot!, _localizer.GetLocalizedString("DialogSetSettingsByDefaultTitle"), _localizer.GetLocalizedString("DialogConfirmText"),
                ContentDialogButton.Primary, _localizer.GetLocalizedString("DialogSetSettingsByDefaultText"), closeBtnText: _localizer.GetLocalizedString("DialogCancelText"));

            return dialogResult == ContentDialogResult.Primary;
        }

        public async Task<bool> ShowOpenReportErrorAsync(string exMessage)
        {
            if (_xamlRoot == null)
                ThrowXamlRootException();

            await ShowContentDialogAsync(_xamlRoot!, _localizer.GetLocalizedString("DialogErrorTitle"), _localizer.GetLocalizedString("DialogOkay"), 
                ContentDialogButton.Primary, $"{_localizer.GetLocalizedString("DialogFailedImportSettingsText")} {exMessage}");
            return true;
        }

        public async Task<bool> ConfirmClosingSettingsMenuAsync()
        {
            if (_xamlRoot == null)
                ThrowXamlRootException();

            var dialogResult = await ShowContentDialogAsync(_xamlRoot!, _localizer.GetLocalizedString("DialogUnsavedChangesTitle"), _localizer.GetLocalizedString("DialogConfirmText"),
                ContentDialogButton.Primary, _localizer.GetLocalizedString("DialogUnsavedChangesText"), closeBtnText: _localizer.GetLocalizedString("DialogCancelText"));

            return dialogResult == ContentDialogResult.Primary;
        }

        public async Task<bool> ShowSettingsImportErrorAsync(string exMessage)
        {
            if (_xamlRoot == null)
                ThrowXamlRootException();

            await ShowContentDialogAsync(_xamlRoot!, _localizer.GetLocalizedString("DialogErrorTitle"), _localizer.GetLocalizedString("DialogOkay"), ContentDialogButton.Primary, 
                $"{_localizer.GetLocalizedString("DialogFailedImportSettingsText")} {exMessage}");
            return true;
        }

        public async Task<bool> ShowTargetSelectionDialogAsync(IServiceProvider serviceProvider)
        {
            if (_xamlRoot == null)
                ThrowXamlRootException();

            var dialogResult =  await ShowContentDialogAsync(_xamlRoot!, _localizer.GetLocalizedString("DialogTargetSelectionTitle"), _localizer.GetLocalizedString("DialogApplyText"),
                ContentDialogButton.Primary, new TargetSelectDialog(serviceProvider), _localizer.GetLocalizedString("DialogCancelText"), null);

            return dialogResult == ContentDialogResult.Primary;
        }

        public async Task<bool> ShowUnsavedChangesWarningAsync()
        {
            if (_xamlRoot == null)
                ThrowXamlRootException();

            await ShowContentDialogAsync(_xamlRoot!, _localizer.GetLocalizedString("DialogExportSettingsWarningTitle"), 
                _localizer.GetLocalizedString("DialogOkay"), ContentDialogButton.Primary, _localizer.GetLocalizedString("DialogExportSettingsWarningText"));
            return true;
        }

        public async Task<bool> ShowUnhandledExceptionAsync(System.Exception exception)
        {
            if (_xamlRoot == null)
                ThrowXamlRootException();

            await ShowContentDialogAsync(_xamlRoot!,
                    "Error", "Okay", ContentDialogButton.Primary, $"{exception.Message}");
            return true;
        }

        private void ThrowXamlRootException()
        {
            throw new InvalidOperationException("XamlRoot hasn't been initialized.");
        }

        private async Task<ContentDialogResult> ShowContentDialogAsync<TContent>(XamlRoot root, string title,
            string primaryBtnText, ContentDialogButton defaultBtn, TContent content, string? secondarybtnText = null, string? closeBtnText = null)
        {
            ContentDialog dialog = new ContentDialog();

            dialog.XamlRoot = root;
            dialog.Title = title;
            dialog.PrimaryButtonText = primaryBtnText;
            dialog.CloseButtonText = closeBtnText;
            dialog.SecondaryButtonText = secondarybtnText;
            dialog.DefaultButton = defaultBtn;
            dialog.Content = content;

            return await dialog.ShowAsync();
        }
    }
}
