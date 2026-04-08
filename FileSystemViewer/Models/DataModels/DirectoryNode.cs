using System.Collections.ObjectModel;

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

        public void UpdateFileCountProperty()
        {
            OnPropertyChanged(nameof(FileCount));
        }
    }
}
