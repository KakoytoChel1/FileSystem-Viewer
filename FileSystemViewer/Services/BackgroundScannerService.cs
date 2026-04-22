using FileSystemViewer.Models;
using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels.Tools;
using FileSystemViewer.Views.Converters;
using Humanizer;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinUI3Localizer;

namespace FileSystemViewer.Services
{
    public class BackgroundScannerService(IDriveUtilsService driveUtilsService, TimeProvider timeProvider, IConfigurationService<AppSettings> configurationService) : IBackgroundScannerService
    {
        private IDriveUtilsService _driveUtilsService = driveUtilsService;
        private TimeProvider _timeProvider = timeProvider;
        private IConfigurationService<AppSettings> _configurationService = configurationService;

        public async Task ProceedScan(ResourceLoader resourceLoader)
        {
            long startTime;
            TimeSpan elapsedTime;
            BytesIntoSuitableFormatConverter bytesConverter = new BytesIntoSuitableFormatConverter();

            ObservableCollection<DriveNode> driveNodes = new ObservableCollection<DriveNode>();
            List<DriveInfo> drives = _driveUtilsService.GetAvailableDrives();

            startTime = _timeProvider.GetTimestamp();

            foreach (DriveInfo driveInfo in drives)
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
                ScanDateTime = DateTime.Now,
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

            string reportPath = ScanningReportHelper.GenerateReportAsJson(scanReport);
            string message = $"{resourceLoader.GetString("NotificationScheduledSuccessText1")} {driveNodes.Count}, {resourceLoader.GetString("NotificationScheduledSuccessText2")} {bytesConverter.Convert(driveNodes.Sum(n => n.Size),
                    typeof(long), null!, null!)}, {resourceLoader.GetString("NotificationScheduledSuccessText3")} {driveNodes.Sum(n => n.FileCount)}, { resourceLoader.GetString("NotificationScheduledSuccessText4")} {driveNodes.Sum(n => n.DirectoriesCount)}.";

            string successImagePath = Path.Combine(AppContext.BaseDirectory, "Assets", "success.png");
            // Basic scanning result notification
            NotificationManager.BuildAndShowToastNotification($"{resourceLoader.GetString("NotificationScheduledSuccessTitle")} {elapsedTime.Humanize()}.", message, successImagePath, reportPath);

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
                StringBuilder messageBuilder = new StringBuilder();

                foreach (string driveName in drivesNames)
                {
                    messageBuilder.Append($"{driveName}; ");
                }

                string warningImagePath = Path.Combine(AppContext.BaseDirectory, "Assets", "warning.png");
                // Low free space warning notification
                NotificationManager.BuildAndShowToastNotification(resourceLoader.GetString("NotificationWarningTitle"), messageBuilder.ToString(), warningImagePath);
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
            Dictionary<DirectoryNode, TotalScanValues> totalScanValues = new Dictionary<DirectoryNode, TotalScanValues>();

            foreach (FileSystemNode node in data)
            {
                DirectoryNode parentNode = (node.ParentNode as DirectoryNode)!;
                long fileSize = 0;
                long directoriesCount = 0;
                long fileCount = 0;

                parentNode.FileSystemNodes!.Add(node);

                if (node is DirectoryNode)
                {
                    directoriesCount++;
                }
                else if (node is FileNode fileNode)
                {
                    fileSize = fileNode.Size;
                    fileCount++;
                }

                if (totalScanValues.TryGetValue(parentNode, out var values))
                {
                    values.TotalSizeInBytes += fileSize;
                    values.TotalDirectoryCount += directoriesCount;
                    values.TotalFileCount += fileCount;
                }
                else
                {
                    values = new TotalScanValues();
                    values.TotalSizeInBytes = fileSize;
                    values.TotalDirectoryCount = directoriesCount;
                    values.TotalFileCount = fileCount;
                    totalScanValues.Add(parentNode, values);
                }
            }

            foreach (KeyValuePair<DirectoryNode, TotalScanValues> pair in totalScanValues)
            {
                var current = pair.Key;

                while (current != null)
                {
                    current.Size += pair.Value.TotalSizeInBytes;
                    current.FileCount += pair.Value.TotalFileCount;
                    current.DirectoriesCount += pair.Value.TotalDirectoryCount;
                    current = current.ParentNode as DirectoryNode;
                }
            }
        }

        //private string SaveScanResultsToFile(ObservableCollection<DriveNode> driveNodes, BytesIntoSuitableFormatConverter bytesConverter, TimeSpan elapsedTime)
        //{
        //    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        //    string appFolder = Path.Combine(documentsPath, "FileSystemViewer");

        //    if (!Directory.Exists(appFolder))
        //    {
        //        Directory.CreateDirectory(appFolder);
        //    }

        //    DateTime now = DateTime.Now;
        //    string timestamp = now.ToString("yyyy-MM-dd_HH-mm-ss");
        //    string fileName = $"ScanResults_{timestamp}.txt";
        //    string filePath = Path.Combine(appFolder, fileName);

        //    StringBuilder reportBuilder = new StringBuilder();

        //    reportBuilder.AppendLine("=== File System Viewer - Scan Results ===");
        //    reportBuilder.AppendLine($"Scan Date: {now:yyyy-MM-dd HH:mm:ss}");
        //    reportBuilder.AppendLine($"Total Elapsed Time: {elapsedTime.Humanize()}");
        //    reportBuilder.AppendLine();

        //    var totalSize = bytesConverter.Convert(driveNodes.Sum(n => n.Size), typeof(long), null!, null!);
        //    long totalFiles = driveNodes.Sum(n => n.FileCount);
        //    long totalDirectories = driveNodes.Sum(n => n.DirectoriesCount);

        //    reportBuilder.AppendLine("=== Overall Summary ===");
        //    reportBuilder.AppendLine($"Drives count: {driveNodes.Count}; Total size: {totalSize}; Total files: {totalFiles}; Total directories: {totalDirectories};");
        //    reportBuilder.AppendLine();

        //    reportBuilder.AppendLine("=== Individual Drives ===");
        //    foreach (DriveNode drive in driveNodes)
        //    {
        //        var driveSize = bytesConverter.Convert(drive.Size, typeof(long), null!, null!);
        //        reportBuilder.AppendLine($"Drive: {drive.Name}; Size: {driveSize}; Files: {drive.FileCount}; Directories: {drive.DirectoriesCount};");
        //    }

        //    File.WriteAllText(filePath, reportBuilder.ToString(), Encoding.UTF8);

        //    return filePath;
        //}
        
        /// <summary>
        /// true if free space is less then constant value, otherwise false.
        /// </summary>
        private bool CheckDriveFreeSpace(DriveNode driveNode)
        {
            double percent = (double)driveNode.Size / driveNode.TotalSize * 100;

            if (percent >= (100 - _configurationService.Settings.MinFreeSpacePercent))
            {
                return true;
            }
            return false;
        }
    }
}
