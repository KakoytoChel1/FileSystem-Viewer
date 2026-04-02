using FileSystem_Viewer.Models.DataModels;
using FileSystem_Viewer.ViewModels;
using FileSystemViewer.Models;
using FileSystemViewer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Windows.UI;

namespace FileSystemViewer.Services
{
    public class DriveUtilsService : IDriveUtilsService
    {
        private IFileExtentionItemService _fileExtentionItemService;

        public DriveUtilsService(IFileExtentionItemService fileExtentionItemService)
        {
            _fileExtentionItemService = fileExtentionItemService;
        }

        public List<DriveInfo> GetAvailableDrives()
        {
            List<DriveInfo> availableDrives = new List<DriveInfo>();

            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo driveInfo in allDrives)
            {
                if (driveInfo.IsReady)
                {
                    availableDrives.Add(driveInfo);
                }
            }

            return availableDrives;
        }

        public async Task ScanProvidedNodesAsync<T>(ObservableCollection<T> nodes, IProgress<List<FileSystemNode>> progress, IProgress<DirectoryNode> completeProgress, CancellationToken cancellationToken, PauseResetToken pauseResetToken) where T : DirectoryNode
        {
            if (nodes == null || !nodes.Any()) { return; }

            var parallelOptions = new ParallelOptions()
            {
                CancellationToken = cancellationToken,
            };

            var scanChannel = Channel.CreateBounded<FileSystemNode>(new BoundedChannelOptions(10000)
            {
                SingleWriter = false,
                SingleReader = true
            });

            List<Task> producerTasks = new List<Task>();

            foreach (DirectoryNode node in nodes)
            {
                var producerTask = Task.Run(async () =>
                {
                    DirectoryNode currentNode = node;

                    try
                    {
                        await Parallel.ForEachAsync(node.FileSystemNodes!, parallelOptions, async (fileSystemNode, ct) =>
                        {
                            if (fileSystemNode is DirectoryNode directoryNode)
                            {
                                await ScanAsync(directoryNode, directoryNode.FullPath, scanChannel.Writer, ct, pauseResetToken);
                            }
                        });
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception) { }

                    completeProgress?.Report(currentNode);
                });

                producerTasks.Add(producerTask);
            }

            var consumerTask = Task.Run(async () => await ProccessAndSendNodesAsync(scanChannel.Reader, progress, cancellationToken));

            await Task.WhenAll(producerTasks);

            scanChannel.Writer.Complete();

            await consumerTask;
        }

        public TotalScanValues ScanDirectoryLevel(DirectoryNode directoryNode, string directoryPath)
        {
            var currentDirectoryInfo = new DirectoryInfo(directoryPath);

            int totalFilesForThisLevel = 0;
            int totalDirectoriesForThisLevel = 0;

            try
            {
                foreach (FileInfo fileInfo in currentDirectoryInfo.EnumerateFiles())
                {
                    try
                    {
                        FileNode fileNode = CreateFileNode(directoryNode, fileInfo);

                        directoryNode.FileSystemNodes!.Add(fileNode);
                        directoryNode.FileCount++;
                        directoryNode.Size += fileInfo.Length;
                        totalFilesForThisLevel++;

                        _fileExtentionItemService.UpdateOrCreateFileExtensionItem(fileNode.Extension, fileNode.Size, 1);

                    }
                    catch (FileNotFoundException) { }
                    catch (Exception) { }
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (Exception) { }

            try
            {
                foreach (DirectoryInfo subDirectoryInfo in currentDirectoryInfo.EnumerateDirectories())
                {
                    DirectoryNode subDirectoryNode = CreateDirectoryNode(directoryNode, subDirectoryInfo);
                    totalDirectoriesForThisLevel++;

                    directoryNode.FileSystemNodes!.Add(subDirectoryNode);
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (Exception ex) when (ex is not OperationCanceledException)
            { }

            return new TotalScanValues() { TotalDirectoryCount = totalDirectoriesForThisLevel, TotalFileCount = totalFilesForThisLevel};
        }

        private async Task ScanAsync(DirectoryNode directoryNode, string directory, ChannelWriter<FileSystemNode> writer, CancellationToken cancellationToken, PauseResetToken pauseResetToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await pauseResetToken.IfPauseRequestedPauseAsync(cancellationToken);

            var currentDirectoryInfo = new DirectoryInfo(directory);

            try
            {
                foreach (FileInfo fileInfo in currentDirectoryInfo.EnumerateFiles())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await pauseResetToken.IfPauseRequestedPauseAsync(cancellationToken);

                    try
                    {
                        FileNode fileNode = CreateFileNode(directoryNode, fileInfo);

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
                foreach (DirectoryInfo subDirectoryInfo in currentDirectoryInfo.EnumerateDirectories())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await pauseResetToken.IfPauseRequestedPauseAsync(cancellationToken);

                    DirectoryNode subDirectoryNode = CreateDirectoryNode(directoryNode, subDirectoryInfo);

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
                await foreach (FileSystemNode node in reader.ReadAllAsync(cancellationToken))
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

        private FileNode CreateFileNode(DirectoryNode parent, FileInfo fileInfo)
        {
            FileNode fileNode = new FileNode(parent)
            {
                Name = fileInfo.Name,
                FullPath = fileInfo.FullName,
                Size = fileInfo.Length,
                LastModified = fileInfo.LastWriteTime,
                Extension = fileInfo.Extension,
                UnicodeIcon = UnicodeManager.FileIcon,
                IconColor = ColorManager.FileIconColor,
                FileCount = 1
            };
            return fileNode;
        }

        private DirectoryNode CreateDirectoryNode(DirectoryNode parent, DirectoryInfo directoryInfo)
        {
            DirectoryNode directoryNode = new DirectoryNode(parent)
            {
                Name = directoryInfo.Name,
                FullPath = directoryInfo.FullName,
                Size = 0,
                LastModified = directoryInfo.LastWriteTime,
                UnicodeIcon = UnicodeManager.DirectoryIcon,
                IconColor= ColorManager.DirectoryIconColor
            };
            return directoryNode;
        }
    }
}
    