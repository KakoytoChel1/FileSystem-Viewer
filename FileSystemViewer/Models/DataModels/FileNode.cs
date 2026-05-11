namespace FileSystemViewer.Models
{
    public class FileNode(FileSystemNode parentNode) : FileSystemNode(parentNode)
    {
        public required string Extension { get; set; }
    }
}
