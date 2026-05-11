using FileSystemViewer.Interfaces;
using Microsoft.Windows.AppLifecycle;
using Moq;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace FileSystemViewer.UnitTests
{
    public class ProgramTests
    {
        [Fact]
        public async Task RecognizeActivationKindAsync_UseFileActivationArgs_CallsFileActivationHandler()
        {
            // Arrange
            var mockActivationHandler = new Mock<IAppActivationHandler>();
            var mockFileActivationArgs = new Mock<IFileActivatedEventArgs>();

            var fakeFiles = new List<IStorageItem>();
            mockFileActivationArgs.Setup(x => x.Files).Returns(fakeFiles);

            // Act
            await Program.RecognizeActivationKindAsync(ExtendedActivationKind.File, mockFileActivationArgs.Object, mockActivationHandler.Object, false);

            // Assert
            mockActivationHandler.Verify(h => h.HandleFileOpenActivation(fakeFiles), Times.Once);
            mockActivationHandler.Verify(h => h.HandleCommandLineActivation(It.IsAny<string[]>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task RecognizeActivationKindAsync_UseLaunchActivationArgs_CallsCommandLineActivationHandler()
        {
            // Arrange
            var mockActivationHandler = new Mock<IAppActivationHandler>();
            var mockLaunchActivationArgs = new Mock<ILaunchActivatedEventArgs>();

            mockLaunchActivationArgs.Setup(x => x.Arguments).Returns(@"fsv.exe --scan C:\\Test");

            // Act
            await Program.RecognizeActivationKindAsync(ExtendedActivationKind.Launch, mockLaunchActivationArgs.Object, mockActivationHandler.Object, true);

            // Assert
            mockActivationHandler.Verify(h => h.HandleCommandLineActivation(
            It.Is<string[]>(arr => arr.Length == 3 && arr[1] == "--scan" && arr[2] == @"C:\\Test"),
            true),
            Times.Once);
        }

        [Fact]
        public async Task RecognizeActivationKindAsync_WhenThrowsException_CatchsException()
        {
            // Arrange
            var mockHandler = new Mock<IAppActivationHandler>();
            var mockStartupArgs = new Mock<IStartupTaskActivatedEventArgs>();

            mockHandler.Setup(h => h.HandleStartupActivation(It.IsAny<IStartupTaskActivatedEventArgs>()))
                   .Throws(new Exception("Some Failure"));

            // Act
            var exception = await Record.ExceptionAsync(
                () => Program.RecognizeActivationKindAsync(ExtendedActivationKind.StartupTask, mockStartupArgs.Object, mockHandler.Object, false));

            // Assert
            Assert.Null(exception);
        }
    }
}
