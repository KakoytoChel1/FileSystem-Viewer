using System;
using System.Collections.ObjectModel;

namespace FileSystemViewer.Models.DataModels
{
    public class ScanReport
    {
        public string Name { get; set; } = "Report";
        public DateTime ScanDateTime { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public int NodesScanned { get; set; }
        public long TotalSize { get; set; }
        public long TotalFilesCount { get; set; }
        public long TotalDirectoriesCount { get; set; }
        public ObservableCollection<DirectoryNode> RootNodes { get; set; } = new ObservableCollection<DirectoryNode>();
    }
}
