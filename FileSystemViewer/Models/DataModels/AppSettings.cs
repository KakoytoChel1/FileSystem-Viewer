using System;
using System.Collections.Generic;

namespace FileSystemViewer.Models.DataModels
{
    public class AppSettings
    {
        public enum ThemeMode
        {
            Light,
            Dark,
            System
        }

        public enum Language
        {
            English,
            Ukrainian
        }

        public bool IsTrayActive { get; set; } = true;
        public bool IsScheduledScanningEnabled { get; set; } = false;
        public TimeSpan ScheduledScanningTime { get; set; } = TimeSpan.FromHours(12);
        public double MinFreeSpacePercent { get; set; } = 10.0;
        public List<int> AccentColor { get; set; } = new List<int>(3);
        public ThemeMode AppTheme { get; set; } = ThemeMode.System;
        public Language AppLanguage { get; set; } = Language.English;
    }
}
