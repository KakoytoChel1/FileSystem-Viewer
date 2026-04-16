using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.IO;

namespace FileSystemViewer.ViewModels.Tools
{
    public static class NotificationManager
    {
        public static void BuildAndShowToastNotification(string title, string content, string? iconFilePath = null, string? reportDocumentPath = null)
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
                builder.AddArgument("action", "openReport")
                       .AddArgument("reportPath", reportDocumentPath);

                builder.AddText("Click for more details...");
            }

            AppNotificationManager.Default.Show(builder.BuildNotification());
        }
    }
}