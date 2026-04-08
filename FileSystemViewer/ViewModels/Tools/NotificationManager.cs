using Microsoft.Toolkit.Uwp.Notifications;
using System;

namespace FileSystemViewer.ViewModels.Tools
{
    public static class NotificationManager
    {
        public static void BuildAndShowToastNotification(string title, string content, string? iconFilePath = null, string? reportDocumentPath = null)
        {
            ToastContentBuilder builder = new ToastContentBuilder()
                .AddText(title)
                .AddText(content);

            if (!string.IsNullOrWhiteSpace(iconFilePath))
            {
                builder.AddAppLogoOverride(new Uri($"file:///{iconFilePath}"), ToastGenericAppLogoCrop.Circle);
            }

            if (!string.IsNullOrWhiteSpace(reportDocumentPath))
            {
                builder.SetProtocolActivation(new Uri($"file:///{reportDocumentPath}"));
                builder.AddAttributionText("Click for more details...");
            }

            builder.Show();
        }
    }
}
