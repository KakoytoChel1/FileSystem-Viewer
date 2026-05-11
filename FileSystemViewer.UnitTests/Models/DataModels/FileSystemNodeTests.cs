using FileSystemViewer.Models;
using FileSystemViewer.UnitTests.Helpers;

namespace FileSystemViewer.UnitTests.Models.DataModels
{
    public class FileSystemNodeTests
    {
        [Theory]
        [InlineData(true, 0, 1024, 100.0)]     // Parent null
        [InlineData(false, 0, 1024, 0.0)]      // Parent size 0
        [InlineData(false, 2048, 1024, 50.0)]  // Valid parent and size
        public void PercentProperty_CalculatesCorrectly(bool isParentNull, long parentSize, long nodeSize, double expectedPercent)
        {
            // Arrange
            FileSystemNodeTest? parentNode = isParentNull ? null : FakeFileSystemNodeHelper.GetFileSystemNodeForTest(null, parentSize);
            FileSystemNodeTest fileSystemNodeTest = FakeFileSystemNodeHelper.GetFileSystemNodeForTest(parent: parentNode, size: nodeSize);

            // Act
            double result = fileSystemNodeTest.PercentProperty;

            // Assert
            Assert.Equal(expectedPercent, result);
        }
    }
}