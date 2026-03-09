using System;

namespace FileSystem_Viewer.Models
{
    public class FileNode : FileSystemNode
    {
        public FileNode(FileSystemNode paretnNode) : base(paretnNode) { }

        public FileNode(FileSystemNode parentNode, string name, string fullPath, long size, DateTime lastModified) : base(parentNode, name, fullPath, size, lastModified)
        {

        }
    }
}
