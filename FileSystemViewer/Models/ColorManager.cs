using System.Collections.Generic;
using Windows.UI;

namespace FileSystemViewer.Models
{
    public static class ColorManager
    {
        private static readonly Color _defaultColor = Color.FromArgb(255, 128, 128, 128);
        private static readonly Color _otherColor = Color.FromArgb(255, 79, 79, 79);
        private static readonly Color _directoryTreemapNodeColor = Color.FromArgb(255, 89, 94, 171);

        private static readonly Color _directoryIconColor = Color.FromArgb(255, 235, 193, 70);
        private static readonly Color _fileIconColor = Color.FromArgb(255, 207, 206, 204);
        private static readonly Color _driveIconColor = Color.FromArgb(255, 89, 94, 171);

        private static readonly Dictionary<string, Color> _extensionColorPairs = new Dictionary<string, Color>()
        {
            {string.Empty, Color.FromArgb(255, 50, 89, 125) },

            {".jpg", Color.FromArgb(255, 56, 161, 199) },
            { ".jpeg", Color.FromArgb(255, 56, 161, 199) },
            { ".png", Color.FromArgb(255, 120, 56, 199) },
            { ".gif", Color.FromArgb(255, 255, 215, 0) },
            { ".svg", Color.FromArgb(255, 230, 76, 25) },
            { ".psd", Color.FromArgb(255, 23, 19, 242) },

            { ".mp4", Color.FromArgb(255, 184, 27, 227) },
            { ".mov", Color.FromArgb(255, 165, 22, 204) },
            { ".avi", Color.FromArgb(255, 169, 39, 204) },
            { ".mkv", Color.FromArgb(255, 148, 6, 186) },

            { ".mp3", Color.FromArgb(255, 191, 31, 111) },
            { ".wav", Color.FromArgb(255, 7, 151, 173) },

            { ".zip", Color.FromArgb(255, 148, 34, 34) },
            { ".rar", Color.FromArgb(255, 143, 39, 39) },
            { ".7z", Color.FromArgb(255, 156, 47, 47) },
            { ".iso", Color.FromArgb(255, 188, 198, 204) },

            { ".exe", Color.FromArgb(255, 39, 122, 72) },
            { ".msi", Color.FromArgb(255, 57, 145, 54) },

            { ".dll", Color.FromArgb(255, 101, 107, 112) },
            { ".sys", Color.FromArgb(255, 90, 98, 105) },
            { ".ini", Color.FromArgb(255, 76, 85, 92) },
            { ".dat", Color.FromArgb(255, 20, 38, 140) },

            {".pdf", Color.FromArgb(255, 230, 85, 85) },
            {".fsvscan", Color.FromArgb(255, 12, 120, 130) }
        };

        public static Color DefaultColor
        {
            get { return _defaultColor; }
        }

        public static Color OtherColor
        {
            get { return _otherColor; }
        }

        public static Color DirectoryTreemapNodeColor
        {
            get { return _directoryTreemapNodeColor; }
        }

        public static Color DirectoryIconColor
        {
            get { return _directoryIconColor; }
        }

        public static Color FileIconColor
        {
            get { return _fileIconColor; }
        }
        public static Color DriveIconColor
        {
            get { return _driveIconColor; }
        }

        public static Dictionary<string, Color> ExtensionColorPairs
        {
            get { return _extensionColorPairs; }
        }

        public static Color GetColorByExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return _extensionColorPairs[string.Empty];
            }

            if (_extensionColorPairs.TryGetValue(extension, out var color))
            {
                return color;
            }

            return _defaultColor;
        }

        public static Color GetFileIconColorByExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return _extensionColorPairs[string.Empty];
            }

            if (_extensionColorPairs.TryGetValue(extension, out var color))
            {
                return color;
            }

            return _fileIconColor;
        }
    }
}
