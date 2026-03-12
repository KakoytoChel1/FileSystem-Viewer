using FileSystemViewer.Models;
using FileSystemViewer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;

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

        public async Task ScanProvidedDrivesAsync(ObservableCollection<DriveNode> diskNodes, IProgress<List<FileSystemNode>> progress, CancellationToken cancellationToken, PauseResetToken pauseResetToken)
        {
            if (diskNodes == null || !diskNodes.Any()) { return; }

            var parallelOptions = new ParallelOptions()
            {
                CancellationToken = cancellationToken,
            };

            var scanChannel = Channel.CreateBounded<FileSystemNode>(new BoundedChannelOptions(10000)
            {
                SingleWriter = true,
                SingleReader = true
            });

            var producerTask = Task.Run(async () =>
            {
                try
                {
                    await Parallel.ForEachAsync(diskNodes, parallelOptions, async (driveNode, cancellationToken) =>
                    {
                        await ScanAsync(driveNode, driveNode.FullPath, scanChannel.Writer, cancellationToken, pauseResetToken);
                    });
                }
                catch (OperationCanceledException) { }
                catch (Exception) { }
                finally
                {
                    scanChannel.Writer.Complete();
                }
            });

            var consumerTask = Task.Run(async () => ProccessAndSendNodesAsync(scanChannel.Reader, progress, cancellationToken));

            await Task.WhenAll(producerTask, consumerTask);
        }

        private async Task ScanAsync(DirectoryNode directoryNode, string directory, ChannelWriter<FileSystemNode> writer, CancellationToken cancellationToken, PauseResetToken pauseResetToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await pauseResetToken.IfPauseRequestedPauseAsync(cancellationToken);

            var CurrentDirectoryInfo = new DirectoryInfo(directory);

            try
            {
                foreach (var fileInfo in CurrentDirectoryInfo.EnumerateFiles())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await pauseResetToken.IfPauseRequestedPauseAsync(cancellationToken);

                    try
                    {
                        FileNode fileNode = new FileNode(
                            parentNode: directoryNode,
                            name: fileInfo.Name,
                            fullPath: fileInfo.FullName,
                            size: fileInfo.Length,
                            lastModified: fileInfo.LastWriteTime);

                        await writer.WriteAsync(fileNode);
                    }
                    catch (FileNotFoundException) { }
                    catch (Exception) { }
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (Exception) { }

            cancellationToken.ThrowIfCancellationRequested();
            await pauseResetToken.IfPauseRequestedPauseAsync(cancellationToken);

            try
            {
                foreach (var subDirectoryInfo in CurrentDirectoryInfo.EnumerateDirectories())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await pauseResetToken.IfPauseRequestedPauseAsync(cancellationToken);

                    DirectoryNode subDirectoryNode = new DirectoryNode(
                            parentNode: directoryNode,
                            name: subDirectoryInfo.Name,
                            fullPath: subDirectoryInfo.FullName,
                            size: 0,
                            lastModified: subDirectoryInfo.LastWriteTime);

                    await writer.WriteAsync(subDirectoryNode);

                    await ScanAsync(subDirectoryNode, subDirectoryInfo.FullName, writer, cancellationToken, pauseResetToken);
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (Exception ex) when (ex is not OperationCanceledException)
            { }
        }

        private async Task ProccessAndSendNodesAsync(ChannelReader<FileSystemNode> reader, IProgress<List<FileSystemNode>> progress, CancellationToken cancellationToken)
        {
            var buffer = new List<FileSystemNode>();
            const int bufferSize = 100;

            try
            {
                await foreach (var node in reader.ReadAllAsync(cancellationToken))
                {
                    buffer.Add(node);

                    if (buffer.Count >= bufferSize)
                    {
                        progress?.Report(new List<FileSystemNode>(buffer));
                        buffer.Clear();
                    }
                }

                if (buffer.Count > 0)
                {
                    progress?.Report(new List<FileSystemNode>(buffer));
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception) { }
        }

        public async Task ScanSpecifiedDirectoryAsync(DirectoryNode directoryNode, CancellationToken token, PauseResetToken pauseResetToken)
        {
            throw new NotImplementedException();
        }
    }
}
    