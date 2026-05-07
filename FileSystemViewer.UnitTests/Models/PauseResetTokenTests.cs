using FileSystemViewer.Models;

namespace FileSystemViewer.UnitTests.Models
{
    public class PauseResetTokenTests
    {
        [Fact]
        public void IfPauseRequestedPauseAsync_UninitializedToken_ReturnsCompletedTask()
        {
            // Arrange
            PauseResetToken defaultToken = default;

            // Act
            var task = defaultToken.IfPauseRequestedPauseAsync(CancellationToken.None);

            // Assert
            Assert.True(task.IsCompletedSuccessfully);
        }
    }
}
