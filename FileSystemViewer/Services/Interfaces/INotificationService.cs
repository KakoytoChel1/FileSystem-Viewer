using System;
using System.Collections.Generic;

namespace FileSystemViewer.Services.Interfaces
{
    public interface INotificationService
    {
        public void ShowSuccessScanningNotification(TimeSpan elapsed, int nodesCount, long directoriesCount, long filesCount, string reportFilePath);
        public void ShowCancelScanningNotification();
        public void ShowLowSpaceWarningNotification(IEnumerable<string> drivesNames);
    }
}
