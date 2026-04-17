using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace FileSystemViewer.Models
{
    public class DirectoryNode(FileSystemNode? parentNode) : FileSystemNode(parentNode)
    {
        private bool _isInProgress;
        public override bool IsInProgress 
        { 
            get { return _isInProgress; }
            set { SetProperty(ref _isInProgress, value); }
        }

        private bool? _isExpanded;
        public override bool? IsExpanded
        {
            get { return _isExpanded; }
            set { SetProperty(ref _isExpanded, value); }
        }

        private ObservableCollection<FileSystemNode>? _fileSystemNodes = new ObservableCollection<FileSystemNode>();
        public override ObservableCollection<FileSystemNode>? FileSystemNodes
        {
            get { return _fileSystemNodes; }
            set { SetProperty(ref _fileSystemNodes, value); }
        }

        public long DirectoriesCount { get; set; }

        public void UpdateFileCountProperty()
        {
            OnPropertyChanged(nameof(FileCount));
        }

        protected override void Open()
        {
            if (Directory.Exists(FullPath))
            {
               
                Process.Start(new ProcessStartInfo
                {
                    FileName = FullPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
        }
    }
}
