using System;

namespace FileSystem_Viewer.Models
{
    public class FileNode : FileSystemNode
    {
        public FileNode()
        {
            NodeType = NodeTypes.File;
        }

        public FileNode(string name, string fullPath, long size, DateTime lastModified) : base(name, fullPath, size, lastModified)
        {
            NodeType = NodeTypes.File;
        }
    }
}
