using System;

namespace FileSystemViewer.Models.DataModels
{
    public class AppSettings
    {
        public bool IsTrayActive { get; set; } = true;
        public bool IsScheduledScanningEnabled { get; set; } = false;
        public TimeSpan ScheduledScanningTime { get; set; } = TimeSpan.FromHours(0);
        public string ScheduledScanningTaskPath { get; set; } = @"FileSystemViewer";
        public double MinFreeSpacePercent { get; set; } = 10.0;
    }
}
