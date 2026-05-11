using FileSystemViewer.Models;
using FileSystemViewer.Models.Tools;
using FileSystemViewer.UnitTests.Helpers;

namespace FileSystemViewer.UnitTests.Models.Tools
{
    public class ScanDataAggregatorTests
    {
        [Fact]
        public void AggregateNodes_CommonScenario_CorrectlyAggregated()
        {
            // Arrange
            DirectoryNode rootDirectory = FakeFileSystemNodeHelper.GetDirectoryNodeForTest(null);
            DirectoryNode subDirectory = FakeFileSystemNodeHelper.GetDirectoryNodeForTest(rootDirectory);

            FileNode file1 = FakeFileSystemNodeHelper.GetFileNodeForTest(subDirectory, 100);
            FileNode file2 = FakeFileSystemNodeHelper.GetFileNodeForTest(subDirectory, 50);
            FileNode file3 = FakeFileSystemNodeHelper.GetFileNodeForTest(rootDirectory, 20);

            List<FileSystemNode> fakeReceivedNodes = new List<FileSystemNode> { subDirectory, file1, file2, file3 };

            // Act
            ScanDataAggregator.AggregateNodes(fakeReceivedNodes);

            // Assert
            Assert.Equal(150, subDirectory.Size);
            Assert.Equal(2, subDirectory.FileCount);
            Assert.Equal(0, subDirectory.DirectoriesCount);

            Assert.Equal(170, rootDirectory.Size);
            Assert.Equal(3, rootDirectory.FileCount);
            Assert.Equal(1, rootDirectory.DirectoriesCount);
        }
    }
}
