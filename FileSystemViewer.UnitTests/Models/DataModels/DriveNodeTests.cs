using FileSystemViewer.Models;

namespace FileSystemViewer.UnitTests.Models.DataModels
{
    public class DriveNodeTests
    {
        [Theory]
        [InlineData(0, 0, "(0%)")]           // TotalSize = 0
        [InlineData(0, 1000, "(0%)")]        // Drive is full
        [InlineData(1000, 1000, "(100%)")]   // Drive is fully free
        [InlineData(1024, 2048, "(50%)")]    // Common scenario
        [InlineData(333, 1000, "(33%)")]     // Round check
        public void Tag_CalculatesCorrectPercentage(long totalfreeSpace, long totalSize, string expectedPercentString)
        {
            // Arrange
            DriveNode driveNode = GetFileSystemNodeForTest(totalfreeSpace, totalSize);

            // Act
            string resultTag = driveNode.Tag;

            // Assert
            Assert.Contains(expectedPercentString, resultTag);
        }

        private DriveNode GetFileSystemNodeForTest(long totalFreeSpace, long totalSize)
        {
            DriveNode fileSystemNodeTest = new DriveNode()
            {
                UnicodeIcon = string.Empty,
                IconColor = default,
                Name = string.Empty,
                FullPath = string.Empty,
                Size = 0,
                LastModified = default,
                TotalFreeSpace = totalFreeSpace,
                TotalSize = totalSize,
            };
            return fileSystemNodeTest;
        }
    }
}
