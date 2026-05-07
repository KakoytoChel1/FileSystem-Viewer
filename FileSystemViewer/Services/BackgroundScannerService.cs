using FileSystemViewer.Models;
using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Models.Tools;
using FileSystemViewer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileSystemViewer.Services
{
    public class BackgroundScannerService(IDriveUtilsService driveUtilsService, TimeProvider timeProvider, IConfigurationService<AppSettings> configurationService, INotificationService notificationService, IReportStorageService reportStorageService) : IBackgroundScannerService
    {
        private IDriveUtilsService _driveUtilsService = driveUtilsService;
        private TimeProvider _timeProvider = timeProvider;
        private IConfigurationService<AppSettings> _configurationService = configurationService;
        private INotificationService _notificationService = notificationService;
        private IReportStorageService _reportStorageService = reportStorageService;

        public async Task ProceedScan()
        {
            long startTime;
            TimeSpan elapsedTime;

            ObservableCollection<DriveNode> driveNodes = new ObservableCollection<DriveNode>();
            List<IDriveInfo> drives = _driveUtilsService.GetAvailableDrives();

            startTime = _timeProvider.GetTimestamp();

            foreach (IDriveInfo driveInfo in drives)
            {
                var name = !string.IsNullOrWhiteSpace(driveInfo.VolumeLabel) ? $"{driveInfo.VolumeLabel} {driveInfo.Name}" : driveInfo.Name;

                DriveNode driveNode = new DriveNode()
                {
                    Name = name,
                    FullPath = driveInfo.RootDirectory.FullName,
                    Size = 0,
                    LastModified = driveInfo.RootDirectory.LastWriteTime,
                    UnicodeIcon = UnicodeManager.DriveIcon,
                    IconColor = ColorManager.DriveIconColor,

                    VolumeName = driveInfo.VolumeLabel,
                    TotalFreeSpace = driveInfo.TotalFreeSpace,
                    TotalSize = driveInfo.TotalSize
                };
                driveNodes.Add(driveNode);
                ProceedScanForSelectedDirectoryLevel(driveNode);
            }

            var progress = new Action<List<FileSystemNode>>(ProcessReceivedScannedNodes);

            await _driveUtilsService.ScanProvidedNodesAsync(driveNodes, progress, new CancellationTokenSource().Token, new PauseResetTokenSource().Token);

            elapsedTime = _timeProvider.GetElapsedTime(startTime);

            ScanReport scanReport = new ScanReport()
            {
                ScanDateTime = _timeProvider.GetLocalNow().DateTime,
                ElapsedTime = elapsedTime,
                NodesScanned = driveNodes.Count,
                TotalSize = driveNodes.Sum(n => n.Size),
                TotalDirectoriesCount = driveNodes.Sum(n => n.DirectoriesCount),
                TotalFilesCount = driveNodes.Sum(n => n.FileCount),
                RootNodes = new ObservableCollection<DirectoryNode>(driveNodes.Select((node) =>
                {
                    node.FileSystemNodes = null;
                    return node;
                }))
            };

            string reportPath = _reportStorageService.SaveReport(scanReport);
            _notificationService.ShowSuccessScanningNotification(elapsedTime, driveNodes.Count, driveNodes.Sum(n => n.DirectoriesCount), driveNodes.Sum(n => n.FileCount), reportPath);

            List<string> drivesNames = new List<string>();
            foreach (DriveNode driveNode in driveNodes)
            {
                if (CheckDriveFreeSpace(driveNode))
                {
                    drivesNames.Add(driveNode.Name);
                }
            }

            if (drivesNames.Any())
            {
                _notificationService.ShowLowSpaceWarningNotification(drivesNames);
            }
        }

        private void ProceedScanForSelectedDirectoryLevel(DirectoryNode directoryNode)
        {
            TotalScanValues values = _driveUtilsService.ScanDirectoryLevel(directoryNode, directoryNode.FullPath);

            var current = directoryNode;
            while (current != null)
            {
                current.Size += values.TotalSizeInBytes;
                current.FileCount += values.TotalFileCount;
                current.DirectoriesCount += values.TotalDirectoryCount;
                current = current.ParentNode as DirectoryNode;
            }
        }

        private void ProcessReceivedScannedNodes(List<FileSystemNode> data)
        {
            ScanDataAggregator.AggregateNodes(data);
        }
        
        /// <summary>
        /// true if free space is less then constant value, otherwise false.
        /// </summary>
        private bool CheckDriveFreeSpace(DriveNode driveNode)
        {
            if (driveNode.TotalSize == 0) return false;

            double percent = (double)driveNode.Size / driveNode.TotalSize * 100;

            if (percent >= (100 - _configurationService.Settings.MinFreeSpacePercent))
            {
                return true;
            }
            return false;
        }
    }
}
