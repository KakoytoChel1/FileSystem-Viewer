using FileSystemViewer.Models;
using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Models.Tools;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileSystemViewer.Services
{
    public class ScanOrchestratorService(IDriveUtilsService driveUtilsService, TimeProvider timeProvider, IReportStorageService reportStorageService, 
        INotificationService notificationService, IFileExtentionItemService fileExtentionItemService) : IScanOrchestratorService
    {
        public async Task<ScanResult> CoordinateScanningOperationAsync<T>(ObservableCollection<T> target, Action<List<FileSystemNode>> progressCallback, CancellationTokenSource cts, PauseResetTokenSource prts) where T : DirectoryNode
        {
            long targetSizeSum;
            long startTime;
            TimeSpan elapsedTime;
            ScanResult result = new ScanResult();

            startTime = timeProvider.GetTimestamp();

            // First level scanning
            foreach (DirectoryNode directoryNode in target)
            {
                directoryNode.IsInProgress = true;
                ProceedScanForSelectedDirectoryLevel(directoryNode);
            }

            // Main Scanning
            await driveUtilsService.ScanProvidedNodesAsync(target, progressCallback, cts.Token, prts.Token);

            elapsedTime = timeProvider.GetElapsedTime(startTime);
            targetSizeSum = target.Sum(dn => dn.Size); // Size sum of all roots

            var extensionItems = new List<FileExtensionItem>();
            foreach (FileExtensionItem item in fileExtentionItemService.GetOrderedExtensionCollection())
            {
                item.UpdateParameters(targetSizeSum);
                extensionItems.Add(item);
            }
            result.ExtensionItems = extensionItems;
            result.TreemapNodes = TreemapBuilderHelper.Build(target).ToList();

            if (cts.IsCancellationRequested)
            {
                notificationService.ShowCancelScanningNotification();
                result.FinalState = AppState.ScanningStates.Canceled;
                return result;
            }

            // Report saving
            long totalScannedDirectories = target.Sum(d => d.DirectoriesCount) + target.Count;
            long totalScannedFiles = target.Sum(d => d.FileCount);

            ScanReport scanReport = new ScanReport()
            {
                ScanDateTime = DateTime.Now,
                ElapsedTime = elapsedTime,
                NodesScanned = target.Count,
                TotalSize = targetSizeSum,
                TotalFilesCount = totalScannedFiles,
                TotalDirectoriesCount = totalScannedDirectories,
                RootNodes = new ObservableCollection<DirectoryNode>(target.Select((node) =>
                {
                    return new DirectoryNode(null)
                    {
                        Name = node.Name,
                        FullPath = node.FullPath,
                        Size = node.Size,
                        LastModified = node.LastModified,
                        UnicodeIcon = node.UnicodeIcon,
                        IconColor = node.IconColor,
                        FileCount = node.FileCount,
                        DirectoriesCount = node.DirectoriesCount,
                    };
                }))
            };

            string reportFilePath = reportStorageService.SaveReport(scanReport);
            notificationService.ShowSuccessScanningNotification(elapsedTime, target.Count, totalScannedDirectories, totalScannedFiles, reportFilePath);

            result.FinalState = AppState.ScanningStates.Completed;
            return result;
        }

        private void ProceedScanForSelectedDirectoryLevel(DirectoryNode directoryNode)
        {
            TotalScanValues values = driveUtilsService.ScanDirectoryLevel(directoryNode, directoryNode.FullPath);

            var current = directoryNode;
            while (current != null)
            {
                current.Size += values.TotalSizeInBytes;
                current.FileCount += values.TotalFileCount;
                current.DirectoriesCount += values.TotalDirectoryCount;
                current = current.ParentNode as DirectoryNode;
            }
        }
    }
}
