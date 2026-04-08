using System.Collections.Generic;

namespace FileSystemViewer.Models
{
    public static class UnicodeManager
    {
        private const string _directoryIcon = "\uE8D5";
        private const string _fileIcon = "\uE729";
        private const string _driveIcon = "\uE958";

        private static readonly Dictionary<string, string> _extensionColorPairs = new Dictionary<string, string>()
        {
            {string.Empty, "\uE8FF" },

            {".jpg", "\uE91B" },
            { ".jpeg", "\uE91B" },
            { ".png", "\uE91B" },
            { ".gif", "\uF4A9" },
            { ".svg", "\uF56E" },
            { ".psd", "\uEABE" },

            { ".mp4", "\uE8B2" },
            { ".mov", "\uE8B2" },
            { ".avi", "\uE8B2" },
            { ".mkv", "\uE8B2" },

            { ".mp3", "\uEC4F" },
            { ".wav", "\uE8D6" },

            { ".zip", "\uF012" },
            { ".rar", "\uE8F1" },
            { ".7z", "\uF012" },
            { ".iso", "\uE958" },

            { ".exe", "\uEB3B" },
            { ".msi", "\uE7B8" },

            { ".dll", "\uE7C3" },
            { ".sys", "\uE7C3" },
            { ".ini", "\uE7C3" },
            { ".dat", "\uE7C3" },
            { ".pdf", "\uEA90" }
        };

        public static string DirectoryIcon
        {
            get { return _directoryIcon; }
        }
        public static string FileIcon
        {
            get { return _fileIcon; }
        }
        public static string DriveIcon
        {
            get { return _driveIcon; }
        }

        public static string GetFileUnicodeByExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return _extensionColorPairs[string.Empty];
            }

            if (_extensionColorPairs.TryGetValue(extension, out var color))
            {
                return color;
            }

            return _fileIcon;
        }
    }
}
