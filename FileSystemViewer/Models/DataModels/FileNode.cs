using System;

namespace FileSystemViewer.Models
{
    public class FileNode : FileSystemNode
    {
        public FileNode(FileSystemNode parentNode) : base(parentNode) { }

        public FileNode(FileSystemNode parentNode, string name, string fullPath, long size, DateTime lastModified) : base(parentNode, name, fullPath, size, lastModified)
        {

        }
    }
}
