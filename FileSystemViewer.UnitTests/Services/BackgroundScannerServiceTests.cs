using FileSystemViewer.Models;
using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Services;
using FileSystemViewer.Services.Interfaces;
using Moq;
using System.Collections.ObjectModel;
using System.IO.Abstractions.TestingHelpers;

namespace FileSystemViewer.UnitTests.Services
{
    public class BackgroundScannerServiceTests
    {
        [Theory]
        [InlineData(1024, 0, 10, 0)]
        [InlineData(1024, 1000, 10, 1)]
        [InlineData(0, 0, 10, 0)]
        public async Task ProceedScan_NormalConditions_CallsCorrectServices(long totalDriveSize, long occupiedSize, double minPercent, int expectedTimes)
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();

            mockFileSystem.AddDrive("X:\\", new MockDriveData
            {
                TotalSize = totalDriveSize
            });

            var mockDriveService = new Mock<IDriveUtilsService>();
            var mockConfigService = new Mock<IConfigurationService<AppSettings>>();
            var mockNotificationService = new Mock<INotificationService>();
            var mockReportService = new Mock<IReportStorageService>();

            var fakeSettings = new AppSettings { MinFreeSpacePercent = minPercent };
            mockConfigService.Setup(c => c.Settings).Returns(fakeSettings);

            var fakeDrivesInfo = mockFileSystem.DriveInfo.GetDrives().ToList();
            mockDriveService.Setup(d => d.GetAvailableDrives()).Returns(fakeDrivesInfo);

            mockDriveService.Setup(d => d.ScanDirectoryLevel(It.IsAny<DirectoryNode>(), It.IsAny<string>()))
                            .Returns(new TotalScanValues { TotalSizeInBytes = occupiedSize });

            // Создаем сам сервис
            var scannerService = new BackgroundScannerService(
                mockDriveService.Object,
                TimeProvider.System,
                mockConfigService.Object,
                mockNotificationService.Object,
                mockReportService.Object);

            // Act
            await scannerService.ProceedScan();

            // Assert

            // Check if scanning has happened
            mockDriveService.Verify(d => d.ScanProvidedNodesAsync(
                It.IsAny<ObservableCollection<DriveNode>>(),
                It.IsAny<Action<List<FileSystemNode>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<PauseResetToken>()),
                Times.Once);

            // If report saved
            mockReportService.Verify(r => r.SaveReport(It.IsAny<ScanReport>()), Times.Once);

            // If notification sent
            mockNotificationService.Verify(n => n.ShowSuccessScanningNotification(
                It.IsAny<TimeSpan>(),
                It.IsAny<int>(),
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<string>()),
                Times.Once);

            mockNotificationService.Verify(n => n.ShowLowSpaceWarningNotification(It.IsAny<List<string>>()), Times.Exactly(expectedTimes));
        }
    }
}
