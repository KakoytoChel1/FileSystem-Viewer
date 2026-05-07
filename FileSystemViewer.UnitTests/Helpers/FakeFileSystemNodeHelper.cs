using FileSystemViewer.Models;

namespace FileSystemViewer.UnitTests.Helpers
{
    public class FileSystemNodeTest : FileSystemNode
    {
        public FileSystemNodeTest(FileSystemNode? parentNode) : base(parentNode) { }
    }

    public class FakeFileSystemNodeHelper
    {
        public static FileSystemNodeTest GetFileSystemNodeForTest(FileSystemNode? parent, long size = 0)
        {
            return new FileSystemNodeTest(parent)
            {
                UnicodeIcon = string.Empty,
                IconColor = default,
                Name = string.Empty,
                FullPath = string.Empty,
                Size = size,
                LastModified = default
            };
        }

        public static DirectoryNode GetDirectoryNodeForTest(FileSystemNode? parent, long size = 0)
        {
            return new DirectoryNode(parent)
            {
                UnicodeIcon = string.Empty,
                IconColor = default,
                Name = string.Empty,
                FullPath = string.Empty,
                Size = size,
                LastModified = default
            };
        }

        public static FileNode GetFileNodeForTest(FileSystemNode parent, long size, string extension = "")
        {
            return new FileNode(parent)
            {
                UnicodeIcon = string.Empty,
                IconColor = default,
                Name = string.Empty,
                FullPath = string.Empty,
                Size = size,
                LastModified = default,
                Extension = extension
            };
        }
    }
}
