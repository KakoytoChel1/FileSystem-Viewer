namespace FileSystemViewer.Models
{
    public static class UnicodeManager
    {
        private readonly static string _directoryIcon = "\uE8D5";
        private readonly static string _fileIcon = "\uE729";
        private readonly static string _driveIcon = "\uE958";

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
    }
}
