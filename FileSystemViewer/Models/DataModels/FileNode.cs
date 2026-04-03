namespace FileSystemViewer.Models
{
    public class FileNode : FileSystemNode
    {
        public FileNode(FileSystemNode parentNode) : base(parentNode) { }
        public string Extension { get; set; } = null!;
    }
}
