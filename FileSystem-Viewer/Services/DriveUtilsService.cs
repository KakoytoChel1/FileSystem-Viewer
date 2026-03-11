using FileSystem_Viewer.Models.DataModels;
using FileSystemViewer.Models;
using FileSystemViewer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileSystemViewer.Services
{
    public class DriveUtilsService : IDriveUtilsService
    {
        public event Action<DirectoryNode, List<FileSystemNode>?, long, long> DrivesUpdated = null!;

        public DriveUtilsService() { }

        public List<DriveInfo> GetAvailableDrives()
        {
            List<DriveInfo> AvailableDrives = new List<DriveInfo>();

            DriveInfo[] AllDrives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in AllDrives)
            {
                AvailableDrives.Add(drive);
            }

            return AvailableDrives;
        }

        public async Task ScanProvidedDrivesAsync(ObservableCollection<DriveNode> diskNodes, CancellationToken token, PauseResetToken pauseResetToken)
        {
            try
            {
                if (diskNodes == null || !diskNodes.Any())
                {
                    //
                    return;
                }

                var parallelOptions = new ParallelOptions()
                {
                    CancellationToken = token,
                };

                await Parallel.ForEachAsync(diskNodes, parallelOptions, async (driveNode, cancellationToken) =>
                {
                    await ScanAsync(driveNode, driveNode.FullPath, cancellationToken, pauseResetToken);
                });
            }
            catch(OperationCanceledException) { }
            catch (Exception) { } 
        }

        public async Task ScanSpecifiedDirectoryAsync(DirectoryNode directoryNode, CancellationToken cancellationToken, PauseResetToken pauseResetToken)
        {
            try
            {
                await Task.Run(async () =>
                {
                    await ScanAsync(directoryNode, directoryNode.FullPath, cancellationToken, pauseResetToken);
                });
            }
            catch (OperationCanceledException) { }
            catch (Exception) { }
        }

        private async Task<TotalScanValues> ScanAsync(DirectoryNode directoryNode, string directory, CancellationToken cancellationToken, PauseResetToken pauseResetToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await pauseResetToken.IfPauseRequestedPauseAsync(cancellationToken);

            var DirectoryBuffer = new List<FileSystemNode>();
            long DirectoryBufferSize = 0;
            long DirectoryBufferFileCount = 0;

            long TotalLevelSize = 0;
            long TotalLevelFileCount = 0;

            var CurrentDirectoryInfo = new DirectoryInfo(directory);

            try
            {
                foreach (var fileInfo in CurrentDirectoryInfo.EnumerateFiles())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await pauseResetToken.IfPauseRequestedPauseAsync(cancellationToken);

                    try
                    {
                        FileNode FileNode = new FileNode(
                            parentNode: directoryNode,
                            name: fileInfo.Name,
                            fullPath: fileInfo.FullName,
                            size: fileInfo.Length,
                            lastModified: fileInfo.LastWriteTime);

                        DirectoryBuffer.Add(FileNode);

                        DirectoryBufferSize += fileInfo.Length;
                        DirectoryBufferFileCount++;

                        TotalLevelSize += fileInfo.Length;
                        TotalLevelFileCount++;
                    }
                    catch (FileNotFoundException) { }
                    catch (Exception) { }
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (Exception) { }

            SendDirectoryBuffer(directoryNode, DirectoryBuffer, ref DirectoryBufferSize, ref DirectoryBufferFileCount);

            cancellationToken.ThrowIfCancellationRequested();
            await pauseResetToken.IfPauseRequestedPauseAsync(cancellationToken);

            try
            {
                foreach (var subDirectoryInfo in CurrentDirectoryInfo.EnumerateDirectories())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await pauseResetToken.IfPauseRequestedPauseAsync(cancellationToken);

                    DirectoryNode SubDirectoryNode = new DirectoryNode(
                            parentNode: directoryNode,
                            name: subDirectoryInfo.Name,
                            fullPath: subDirectoryInfo.FullName,
                            size: 0,
                            lastModified: subDirectoryInfo.LastWriteTime);

                    DirectoryBuffer.Add(SubDirectoryNode);

                    SendDirectoryBuffer(directoryNode, DirectoryBuffer, ref DirectoryBufferSize, ref DirectoryBufferFileCount);

                    TotalScanValues totals = await ScanAsync(SubDirectoryNode, subDirectoryInfo.FullName, cancellationToken, pauseResetToken);

                    TotalLevelSize += totals.TotalSize;
                    TotalLevelFileCount += totals.TotalFileCount;

                    DrivesUpdated?.Invoke(directoryNode, null, totals.TotalSize, totals.TotalFileCount);

                }
            }
            catch (UnauthorizedAccessException) { }
            catch (Exception ex) when (ex is not OperationCanceledException)
            { }

            return new TotalScanValues(TotalLevelFileCount, TotalLevelSize);
        }

        private void SendDirectoryBuffer(DirectoryNode directoryNode, List<FileSystemNode> directoryBuffer, ref long directorySize, ref long directoryFileCount)
        {
            if (directoryBuffer.Count == 0 && directorySize == 0 && directoryFileCount == 0) return;

            var ItemsToSend = new List<FileSystemNode>(directoryBuffer); // Создаем копию буфера для отправки, чтобы избежать проблем во время его очистки
            DrivesUpdated?.Invoke(directoryNode, ItemsToSend, directorySize, directoryFileCount);

            directoryBuffer.Clear();
            directorySize = 0;
            directoryFileCount = 0;
        }
    }
}
