using Humanizer;
using Microsoft.Windows.ApplicationModel.Resources;

namespace FileSystemViewer.Models
{
    public class DriveNode() : DirectoryNode(null)
    {
        public string? VolumeName { get; set; }
        public required long TotalSize { get; set; }
        public required long TotalFreeSpace { get; set; }

        public override string Tag
        {
            get { return $"{TotalFreeSpace.Bytes().Humanize()} / {TotalSize.Bytes().Humanize()} {GetPercentString()}"; }
        }

        private string GetPercentString()
        {
            double result = (double)TotalFreeSpace / TotalSize;
            return $"({result:P0})";
        }
    }
}
