using FileSystem_Viewer.Models;
using FileSystem_Viewer.Services.IServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileSystem_Viewer.Services
{
    public class DriveUtilsService : IDriveUtilsService
    {
        public event Action<DirectoryNode, List<FileSystemNode>?, long> DrivesUpdated = null!;
        private IDispatcherQueueProvider _dispatcherQueueProvider;

        public DriveUtilsService(IDispatcherQueueProvider dispatcherQueueProvider)
        {
            _dispatcherQueueProvider = dispatcherQueueProvider;
        }

        public List<DriveInfo> GetAvailableDrives()
        {
            List<DriveInfo> availableDrives = new List<DriveInfo>();

            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in allDrives)
            {
                availableDrives.Add(drive);
            }

            return availableDrives;
        }

        public async Task ScanProvidedDrives(ObservableCollection<DirectoryNode> diskNodes, CancellationToken token, PauseResetToken pauseResetToken)
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

                await Parallel.ForEachAsync(diskNodes, parallelOptions, async (dirNode, cancelToken) =>
                {
                    Stopwatch stopWatch = Stopwatch.StartNew();
                    await Scan(dirNode, dirNode.FullPath, cancelToken, stopWatch, pauseResetToken);
                });
            }
            catch(OperationCanceledException) { }
            catch (Exception) { } 
        }

        public async Task ScanSpecifiedDirectory(DirectoryNode directoryNode, CancellationToken cancellationToken, PauseResetToken pauseResetToken)
        {
            try
            {
                await Task.Run(async () =>
                {
                    Stopwatch stopWatch = Stopwatch.StartNew();
                    await Scan(directoryNode, directoryNode.FullPath, cancellationToken, stopWatch, pauseResetToken);
                });
            }
            catch (OperationCanceledException) { }
            catch (Exception) { }
        }

        private async Task Scan(DirectoryNode directoryNode, string directory, CancellationToken cancellationToken, Stopwatch stopwatch, PauseResetToken pauseResetToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await pauseResetToken.IfPauseRequestedPauseAsync(cancellationToken);

            var directoryBuffer = new List<FileSystemNode>();
            long directorySize = 0;

            var currentDirectoryInfo = new DirectoryInfo(directory);

            try
            {
                foreach (var fileInfo in currentDirectoryInfo.EnumerateFiles())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await pauseResetToken.IfPauseRequestedPauseAsync(cancellationToken);

                    try
                    {
                        FileNode fileNode = new FileNode(
                            name: fileInfo.Name,
                            fullPath: fileInfo.FullName,
                            size: fileInfo.Length,
                            lastModified: fileInfo.LastWriteTime);

                        directoryBuffer.Add(fileNode);
                        directorySize += fileInfo.Length;

                        if (stopwatch.ElapsedMilliseconds > 100)
                        {
                            SendDirectoryBuffer(directoryNode, directoryBuffer, ref directorySize, stopwatch);
                        }
                    }
                    catch (FileNotFoundException) { }
                    catch (Exception) { }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            { }

            SendDirectoryBuffer(directoryNode, directoryBuffer, ref directorySize, stopwatch);

            cancellationToken.ThrowIfCancellationRequested();
            await pauseResetToken.IfPauseRequestedPauseAsync(cancellationToken);

            try
            {
                foreach (var subDirectoryInfo in currentDirectoryInfo.EnumerateDirectories())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await pauseResetToken.IfPauseRequestedPauseAsync(cancellationToken);

                    DirectoryNode subDirectoryNode = new DirectoryNode(
                            name: subDirectoryInfo.Name,
                            fullPath: subDirectoryInfo.FullName,
                            size: 0,
                            lastModified: subDirectoryInfo.LastWriteTime);

                    directoryBuffer.Add(subDirectoryNode);

                    SendDirectoryBuffer(directoryNode, directoryBuffer, ref directorySize, stopwatch);

                    await Scan(subDirectoryNode, subDirectoryInfo.FullName, cancellationToken, stopwatch, pauseResetToken);

                    DrivesUpdated?.Invoke(directoryNode, null, subDirectoryNode.Size);
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (Exception ex) when (ex is not OperationCanceledException)
            { }
        }

        private void SendDirectoryBuffer(DirectoryNode directoryNode, List<FileSystemNode> directoryBuffer, ref long directorySize, Stopwatch stopWatch)
        {
            if (directoryBuffer.Count == 0 && directorySize == 0) return;

            var itemsToSend = new List<FileSystemNode>(directoryBuffer); // Создаем копию буфера для отправки, чтобы избежать проблем во время его очистки
            DrivesUpdated?.Invoke(directoryNode, itemsToSend, directorySize);

            directoryBuffer.Clear();
            directorySize = 0;
            stopWatch.Restart();
        }
    }
}
