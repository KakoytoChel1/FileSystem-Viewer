using Microsoft.Toolkit.Uwp.Notifications;
using System;

namespace FileSystemViewer.ViewModels.Tools
{
    public static class NotificationManager
    {
        public static void BuildAndShowToastNotification(string title, string[] contentLines, string? iconFilePath = null)
        {
            ToastContentBuilder builder = new ToastContentBuilder()
                .AddText(title);

            foreach (var line in contentLines)
            {
                builder.AddText(line);
            }

            if (!string.IsNullOrWhiteSpace(iconFilePath))
            {
                builder.AddAppLogoOverride(new Uri($"file:///{iconFilePath}"), ToastGenericAppLogoCrop.Circle);
            }

            builder.Show();
        }
    }
}
