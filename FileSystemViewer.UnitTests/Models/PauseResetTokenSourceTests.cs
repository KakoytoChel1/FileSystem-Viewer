using FileSystemViewer.Models;

namespace FileSystemViewer.UnitTests.Models
{
    public class PauseResetTokenSourceTests
    {
        [Fact]
        public void WaitIfPausedAsync_NotPaused_ReturnsCompletedTask()
        {
            // Arrange
            var source = new PauseResetTokenSource();

            // Act
            var task = source.WaitIfPausedAsync(CancellationToken.None);

            // Assert
            Assert.True(task.IsCompletedSuccessfully);
            Assert.False(source.IsPauseRequested);
        }

        [Fact]
        public async Task WaitIfPausedAsync_WhenPaused_BlocksExecution()
        {
            // Arrange
            var source = new PauseResetTokenSource();
            source.Pause();

            // Act
            var task = source.WaitIfPausedAsync(CancellationToken.None);

            // Assert
            Assert.False(task.IsCompleted);
            Assert.True(source.IsPauseRequested);

            var delayTask = Task.Delay(500);
            var completedTask = await Task.WhenAny(task, delayTask);

            Assert.Equal(delayTask, completedTask); // make sure that the delay task is completed
        }

        [Fact]
        public async Task Reset_WhenPausedReset_UnblocksWaitingTask()
        {
            // Arrange
            var source = new PauseResetTokenSource();
            source.Pause();
            var task = source.WaitIfPausedAsync(CancellationToken.None);

            // Act
            source.Reset();

            // Assert
            var delayTask = Task.Delay(500);
            var completedTask = await Task.WhenAny(task, delayTask);

            Assert.Equal(task, completedTask); // make sure that the paused task is completed
            Assert.True(task.IsCompletedSuccessfully);
            Assert.False(source.IsPauseRequested);
        }

        [Fact]
        public async Task WaitIfPausedAsync_CancellationRequestedWhilePaused_ThrowsTaskCanceledException()
        {
            // Arrange
            var source = new PauseResetTokenSource();
            using var cts = new CancellationTokenSource();

            source.Pause();
            var waitingTask = source.WaitIfPausedAsync(cts.Token);

            // Act
            cts.Cancel();

            // Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waitingTask);
        }
    }
}
