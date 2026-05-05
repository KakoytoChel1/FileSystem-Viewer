using FileSystemViewer.Services.Interfaces;
using Humanizer;
using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileSystemViewer.Services
{
    public class NotificationService : INotificationService
    {
        private readonly string _successImagePath;
        private readonly string _cancelImagePath;
        private readonly string _warningImagePath;
        private ResourceLoader _resourceLoader = new ResourceLoader();

        public NotificationService()
        {
            _successImagePath = Path.Combine(AppContext.BaseDirectory, "Assets", "success.png");
            _cancelImagePath = Path.Combine(AppContext.BaseDirectory, "Assets", "cancel.png");
            _warningImagePath = Path.Combine(AppContext.BaseDirectory, "Assets", "warning.png");
        }
        
        public void ShowSuccessScanningNotification(TimeSpan elapsedTime, int nodesCount, long directoriesCount, long filesCount, string reportFilePath)
        {
            RefreshResourceLoader();
            string message = $"{_resourceLoader.GetString("NotificationSuccessText1")} {directoriesCount}; {_resourceLoader.GetString("NotificationSuccessText2")} " +
                $"{filesCount};\n{_resourceLoader.GetString("NotificationSuccessText3")} {elapsedTime.Humanize()}.";
            BuildAndShowToastNotification($"{_resourceLoader.GetString("NotificationSuccessTitle")} {elapsedTime.Humanize()}.", message, _successImagePath, reportFilePath);
        }

        public void ShowCancelScanningNotification()
        {
            RefreshResourceLoader();
            BuildAndShowToastNotification(
                    _resourceLoader.GetString("NotificationCanceledTitle"),
                    _resourceLoader.GetString("NotificationCanceledText"),
                    _cancelImagePath);
        }

        public void ShowLowSpaceWarningNotification(IEnumerable<string> drivesNames)
        {
            RefreshResourceLoader();

            StringBuilder messageBuilder = new StringBuilder();
            foreach (string driveName in drivesNames)
            {
                messageBuilder.Append($"{driveName}; ");
            }
            BuildAndShowToastNotification(_resourceLoader.GetString("NotificationWarningTitle"), messageBuilder.ToString(), _warningImagePath);
        }

        private void RefreshResourceLoader()
        {
            _resourceLoader = new ResourceLoader();
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
