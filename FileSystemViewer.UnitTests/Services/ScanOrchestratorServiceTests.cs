using FileSystemViewer.Models;
using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Services;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.UnitTests.Helpers;
using FileSystemViewer.ViewModels;
using Moq;
using System.Collections.ObjectModel;

namespace FileSystemViewer.UnitTests.Services
{
    public class ScanOrchestratorServiceTests
    {
        [Fact]
        public async Task CoordinateScanningOperationAsync_CompletedScan_ReturnsCompletedStateAndSavesReport()
        {
            // Arrange
            var mockDriveService = new Mock<IDriveUtilsService>();
            var mockReportService = new Mock<IReportStorageService>();
            var mockNotificationService = new Mock<INotificationService>();
            var mockExtensionService = new Mock<IFileExtentionItemService>();

            var orchestrator = new ScanOrchestratorService(
                mockDriveService.Object,
                TimeProvider.System,
                mockReportService.Object,
                mockNotificationService.Object,
                mockExtensionService.Object);

            var rootNode = FakeFileSystemNodeHelper.GetDirectoryNodeForTest(null, 0, "C://");
            var target = new ObservableCollection<DirectoryNode> { rootNode };

            mockDriveService.Setup(d => d.ScanDirectoryLevel(It.IsAny<DirectoryNode>(), It.IsAny<string>()))
                .Returns(new TotalScanValues { TotalSizeInBytes = 1024, TotalFileCount = 10, TotalDirectoryCount = 2 });
            mockExtensionService.Setup(e => e.GetOrderedExtensionCollection()).Returns(new List<FileExtensionItem>());

            using var cts = new CancellationTokenSource();
            var prts = new PauseResetTokenSource();
            Action<List<FileSystemNode>> dummyProgress = (list) => { };

            // Act
            var result = await orchestrator.CoordinateScanningOperationAsync(target, dummyProgress, cts, prts);

            // Assert
            Assert.Equal(AppState.ScanningStates.Completed, result.FinalState);

            // Check data aggregation
            Assert.Equal(1024, rootNode.Size);
            Assert.Equal(10, rootNode.FileCount);
            Assert.Equal(2, rootNode.DirectoriesCount);

            mockDriveService.Verify(d => d.ScanProvidedNodesAsync(target, dummyProgress, cts.Token, prts.Token), Times.Once);
            mockReportService.Verify(r => r.SaveReport(It.IsAny<ScanReport>()), Times.Once);
            mockNotificationService.Verify(n => n.ShowSuccessScanningNotification(It.IsAny<TimeSpan>(), 1, It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()), Times.Once);
            mockNotificationService.Verify(n => n.ShowCancelScanningNotification(), Times.Never);
        }

        [Fact]
        public async Task CoordinateScanningOperationAsync_CanceledScan_ReturnsCanceledStateAndDoesNotSaveReport()
        {
            // Arrange
            var mockDriveService = new Mock<IDriveUtilsService>();
            var mockReportService = new Mock<IReportStorageService>();
            var mockNotificationService = new Mock<INotificationService>();
            var mockExtService = new Mock<IFileExtentionItemService>();

            var orchestrator = new ScanOrchestratorService(
                mockDriveService.Object,
                TimeProvider.System,
                mockReportService.Object,
                mockNotificationService.Object,
                mockExtService.Object);

            var rootNode = FakeFileSystemNodeHelper.GetDirectoryNodeForTest(null, 0, "C://");
            var target = new ObservableCollection<DirectoryNode> { rootNode };

            mockDriveService.Setup(d => d.ScanDirectoryLevel(It.IsAny<DirectoryNode>(), It.IsAny<string>()))
                .Returns(new TotalScanValues());

            mockExtService.Setup(e => e.GetOrderedExtensionCollection()).Returns(new List<FileExtensionItem>());

            using var cts = new CancellationTokenSource();
            var prts = new PauseResetTokenSource();

            cts.Cancel();

            // Act
            var result = await orchestrator.CoordinateScanningOperationAsync(target, _ => { }, cts, prts);

            // Assert
            Assert.Equal(AppState.ScanningStates.Canceled, result.FinalState);

            mockReportService.Verify(r => r.SaveReport(It.IsAny<ScanReport>()), Times.Never);

            mockNotificationService.Verify(n => n.ShowCancelScanningNotification(), Times.Once);
            mockNotificationService.Verify(n => n.ShowSuccessScanningNotification(It.IsAny<TimeSpan>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()), Times.Never);
        }
    }
}
