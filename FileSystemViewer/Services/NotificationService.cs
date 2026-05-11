using FileSystemViewer.Services.Interfaces;
using Humanizer;
using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WinUI3Localizer;

namespace FileSystemViewer.Services
{
    public class NotificationService : INotificationService
    {
        private readonly string _successImagePath;
        private readonly string _cancelImagePath;
        private readonly string _warningImagePath;
        public ILocalizer _localizer => Localizer.Get();

        public NotificationService()
        {
            _successImagePath = Path.Combine(AppContext.BaseDirectory, "Assets", "success.png");
            _cancelImagePath = Path.Combine(AppContext.BaseDirectory, "Assets", "cancel.png");
            _warningImagePath = Path.Combine(AppContext.BaseDirectory, "Assets", "warning.png");
        }
        
        public void ShowSuccessScanningNotification(TimeSpan elapsedTime, int nodesCount, long directoriesCount, long filesCount, string reportFilePath)
        {
            string message = $"{_localizer.GetLocalizedString("NotificationSuccessText1")} {directoriesCount}; {_localizer.GetLocalizedString("NotificationSuccessText2")} " +
                $"{filesCount};\n{_localizer.GetLocalizedString("NotificationSuccessText3")} {elapsedTime.Humanize()}.";
            BuildAndShowToastNotification($"{_localizer.GetLocalizedString("NotificationSuccessTitle")}.", message, _successImagePath, reportFilePath);
        }

        public void ShowCancelScanningNotification()
        {
            BuildAndShowToastNotification(
                    _localizer.GetLocalizedString("NotificationCanceledTitle"),
                    _localizer.GetLocalizedString("NotificationCanceledText"),
                    _cancelImagePath);
        }

        public void ShowLowSpaceWarningNotification(IEnumerable<string> drivesNames)
        {
            StringBuilder messageBuilder = new StringBuilder();
            foreach (string driveName in drivesNames)
            {
                messageBuilder.Append($"{driveName}; ");
            }
            BuildAndShowToastNotification(_localizer.GetLocalizedString("NotificationWarningTitle"), messageBuilder.ToString(), _warningImagePath);
        }

        private void BuildAndShowToastNotification(string title, string content, string? iconFilePath = null, string? reportDocumentPath = null)
        {
            var builder = new AppNotificationBuilder()
                .AddText(title)
                .AddText(content);

            if (!string.IsNullOrWhiteSpace(iconFilePath) && File.Exists(iconFilePath))
            {
                string fileUri = iconFilePath.StartsWith("file://")
                    ? iconFilePath
                    : $"file:///{iconFilePath.Replace('\\', '/')}";

                builder.SetAppLogoOverride(new Uri(fileUri), AppNotificationImageCrop.Circle);
            }

            if (!string.IsNullOrWhiteSpace(reportDocumentPath))
            {
                builder.AddArgument("reportPath", reportDocumentPath);
                ResourceLoader resourceLoader = new ResourceLoader();

                if (resourceLoader != null)
                {
                    builder.AddText(resourceLoader.GetString("NotificationClickForMore"));
                }
            }
            AppNotificationManager.Default.Show(builder.BuildNotification());
        }
    }
}
