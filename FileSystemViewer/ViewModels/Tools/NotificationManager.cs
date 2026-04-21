using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.IO;
using WinUI3Localizer;

namespace FileSystemViewer.ViewModels.Tools
{
    public static class NotificationManager
    {
        private static ILocalizer _localizer = Localizer.Get();

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

                if (_localizer != null)
                {
                    builder.AddText(_localizer.GetLocalizedString("NotificationClickForMore"));
                }
            }

            AppNotificationManager.Default.Show(builder.BuildNotification());
        }
    }
}