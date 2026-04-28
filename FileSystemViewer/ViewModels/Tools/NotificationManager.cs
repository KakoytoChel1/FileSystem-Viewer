using Microsoft.Windows.ApplicationModel.Resources;
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