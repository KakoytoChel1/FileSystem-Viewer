using System;

namespace FileSystemViewer.Models
{
    public class FileNode : FileSystemNode
    {
        public FileNode(FileSystemNode parentNode) : base(parentNode) { }

        public FileNode(FileSystemNode parentNode, string name, string fullPath, long size, string extension, DateTime lastModified) : base(parentNode, name, fullPath, size, lastModified)
        {
            Extension = extension;
        }

        public string Extension { get; set; } = null!;
    }
}
