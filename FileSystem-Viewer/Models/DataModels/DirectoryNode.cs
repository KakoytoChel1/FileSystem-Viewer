using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FileSystemViewer.Models
{
    public class DirectoryNode : FileSystemNode
    {
        public DirectoryNode(FileSystemNode? parentNode) : base(parentNode) { }

        public DirectoryNode(FileSystemNode? parentNode, string name, string fullPath, long size, DateTime lastModified) : base(parentNode, name, fullPath, size, lastModified) { }

        public DirectoryNode(FileSystemNode? parentNode, string name, string fullPath, long size, DateTime lastModified, IEnumerable<FileSystemNode> fileSystemNodes) : base(parentNode, name, fullPath, size, lastModified)
        {

            _fileSystemNodes = new ObservableCollection<FileSystemNode>(fileSystemNodes);
        }

        private ObservableCollection<FileSystemNode> _fileSystemNodes = new ObservableCollection<FileSystemNode>();
        public ObservableCollection<FileSystemNode> FileSystemNodes
        {
            get { return _fileSystemNodes; }
            set { SetProperty(ref _fileSystemNodes, value); }
        }

        private long _fileCount;
        public long FileCount
        {
            get { return _fileCount; }
            set { SetProperty(ref _fileCount, value); }
        }
    }
}
