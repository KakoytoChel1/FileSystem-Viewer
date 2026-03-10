using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FileSystem_Viewer.Models
{
    public class DirectoryNode : FileSystemNode
    {
        public DirectoryNode() { }

        public DirectoryNode(string name, string fullPath, long size, DateTime lastModified) : base(name, fullPath, size, lastModified) { }

        public DirectoryNode(string name, string fullPath, long size, DateTime lastModified, IEnumerable<FileSystemNode> fileSystemNodes) : base(name, fullPath, size, lastModified)
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
