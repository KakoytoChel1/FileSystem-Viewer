using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FileSystem_Viewer.Models
{
    public class DirectoryNode : FileSystemNode
    {
        public DirectoryNode(string name, string fullPath, long size, DateTime lastModified) : base(name, fullPath, size, lastModified) { }

        public DirectoryNode(string name, string fullPath, long size, DateTime lastModified, IEnumerable<FileNode> fileNodes) : base(name, fullPath, size, lastModified)
        {
            _fileNodes = new ObservableCollection<FileNode>(fileNodes);
        }

        private ObservableCollection<FileNode>? _fileNodes = null;
        public ObservableCollection<FileNode>? FileNodes
        {
            get { return _fileNodes; }
            set { SetProperty(ref _fileNodes, value); }
        }
    }
}
