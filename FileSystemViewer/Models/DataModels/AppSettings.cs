using Microsoft.UI;
using System;
using Windows.UI;

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
        public bool IsStartup { get; set; } = false;
        public bool IsScheduledScanningEnabled { get; set; } = false;
        public TimeSpan ScheduledScanningTime { get; set; } = TimeSpan.FromHours(12);
        public double MinFreeSpacePercent { get; set; } = 10.0;
        public bool IsSystemAccentColorUsed { get; set; } = true;
        public Color CustomAccentColor { get; set; } = Colors.DarkCyan;
        public ThemeMode AppTheme { get; set; } = ThemeMode.System;
        public Language AppLanguage { get; set; } = Language.English;
    }
}
