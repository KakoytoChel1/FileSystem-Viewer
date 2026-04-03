using Humanizer;

namespace FileSystemViewer.Models
{
    public class DriveNode : DirectoryNode
    {
        public DriveNode() : base(null) { }

        public string? VolumeName { get; set; }

        private long _totalSize;
        public long TotalSize
        {
            get { return _totalSize; }
            set { _totalSize = value; OnPropertyChanged(nameof(Tag)); }
        }

        private long _totalFreeSpace;
        public long TotalFreeSpace
        {
            get { return _totalFreeSpace; }
            set { _totalFreeSpace = value; OnPropertyChanged(nameof(Tag)); }
        }

        public override string Tag
        {
            get { return $"{TotalFreeSpace.Bytes().Humanize()} free of {TotalSize.Bytes().Humanize()} {GetPercentString()}"; }
        }

        private string GetPercentString()
        {
            double result = (double)TotalFreeSpace / TotalSize;
            return $"({result:P0})";
        }
    }
}
