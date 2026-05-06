using FileSystemViewer.Models;

namespace FileSystemViewer.UnitTests.Models.DataModels
{
    public class FileSystemNodeTests
    {
        class FileSystemNodeTest : FileSystemNode
        {
            public FileSystemNodeTest(FileSystemNode? parentNode) : base(parentNode) { }
        }

        [Theory]
        [InlineData(true, 0, 1024, 100.0)]     // Parent null
        [InlineData(false, 0, 1024, 0.0)]      // Parent size 0
        [InlineData(false, 2048, 1024, 50.0)]  // Valid parent and size
        public void PercentProperty_CalculatesCorrectly(bool isParentNull, long parentSize, long nodeSize, double expectedPercent)
        {
            // Arrange
            FileSystemNodeTest? parentNode = isParentNull ? null : GetFileSystemNodeForTest(null, parentSize);
            FileSystemNodeTest fileSystemNodeTest = GetFileSystemNodeForTest(parent: parentNode, size: nodeSize);

            // Act
            double result = fileSystemNodeTest.PercentProperty;

            // Assert
            Assert.Equal(expectedPercent, result);
        }

        private FileSystemNodeTest GetFileSystemNodeForTest(FileSystemNode? parent, long size)
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
    }
}