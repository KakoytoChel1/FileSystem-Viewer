using FileSystem_Viewer.Models.DataModels;
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

namespace FileSystemViewer.Services
{
    public class DriveUtilsService : IDriveUtilsService
    {
        private readonly IFileExtentionItemService _fileExtentionItemService;

        public DriveUtilsService(IFileExtentionItemService fileExtentionItemService)
        {
            _fileExtentionItemService = fileExtentionItemService;
        }

        private class WorkCounter
        {
            public int ActiveItems = 0;
        }

        private struct ScanTask
        {
            public DirectoryNode ParentNode { get; }
            public string DirectoryPath { get; }

            public ScanTask(DirectoryNode parentNode, string directoryPath)
            {
                ParentNode = parentNode;
                DirectoryPath = directoryPath;
            }
        }

        public async Task ScanProvidedNodesAsync<T>(ObservableCollection<T> nodes, IProgress<List<FileSystemNode>> progress, CancellationToken cancellationToken, PauseResetToken pauseResetToken) where T : DirectoryNode
        {
            if (nodes == null || !nodes.Any()) { return; }

            // Reads results from workers and sends them to UI
            var resultReaderChannel = Channel.CreateBounded<FileSystemNode>(new BoundedChannelOptions(50000)
            {
                SingleWriter = false,
                SingleReader = true,
                FullMode = BoundedChannelFullMode.Wait
            });

            var workerScannerChannel = Channel.CreateUnbounded<ScanTask>(new UnboundedChannelOptions
            {
                SingleWriter = false,
                SingleReader = false
            });

            var counter = new WorkCounter();

            var consumerTask = Task.Run(() => ProccessAndSendNodesAsync(resultReaderChannel.Reader, progress, cancellationToken));

            // Fills worker channel with initial tasks (root directories of each drive) and sets counter to initial value
            foreach (DirectoryNode node in nodes)
            {
                if (node.FileSystemNodes != null)
                {
                    foreach (var childNode in node.FileSystemNodes.OfType<DirectoryNode>())
                    {
                        Interlocked.Increment(ref counter.ActiveItems);
                        workerScannerChannel.Writer.TryWrite(new ScanTask(childNode, childNode.FullPath));
                    }
                }
            }

            // If we found nothing, comlete and exit
            if (counter.ActiveItems == 0)
            {
                resultReaderChannel.Writer.TryComplete();
                return;
            }

            int workerCount = Environment.ProcessorCount;
            var workerTasks = new Task[workerCount];

            for (int i = 0; i < workerCount; i++)
            {
                workerTasks[i] = Task.Run(() => WorkerLoopAsync(workerScannerChannel, resultReaderChannel.Writer, counter, cancellationToken, pauseResetToken));
            }

            await Task.WhenAll(workerTasks);
            resultReaderChannel.Writer.TryComplete();
            await consumerTask;
        }

        private async Task WorkerLoopAsync(Channel<ScanTask> workChannel, ChannelWriter<FileSystemNode> resultsWriter, WorkCounter counter, CancellationToken cancellationToken, PauseResetToken pauseResetToken)
        {
            await foreach (var task in workChannel.Reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    await pauseResetToken.IfPauseRequestedPauseAsync(cancellationToken);

                    var currentDirectoryInfo = new DirectoryInfo(task.DirectoryPath);

                    foreach (FileInfo fileInfo in currentDirectoryInfo.EnumerateFiles())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        FileNode fileNode = CreateFileNode(task.ParentNode, fileInfo);

                        if (!resultsWriter.TryWrite(fileNode))
                        {
                            await resultsWriter.WriteAsync(fileNode, cancellationToken);
                        }
                    }

                    foreach (DirectoryInfo subDirectoryInfo in currentDirectoryInfo.EnumerateDirectories())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        DirectoryNode subDirectoryNode = CreateDirectoryNode(task.ParentNode, subDirectoryInfo);

                        if (!resultsWriter.TryWrite(subDirectoryNode))
                        {
                            await resultsWriter.WriteAsync(subDirectoryNode, cancellationToken);
                        }

                        // We found a new directory, fix it like a new task for workers and increase counter
                        Interlocked.Increment(ref counter.ActiveItems);
                        workChannel.Writer.TryWrite(new ScanTask(subDirectoryNode, subDirectoryInfo.FullName));
                    }
                }
                catch (UnauthorizedAccessException) { }
                catch (Exception ex) when (ex is not OperationCanceledException) { }
                finally
                {
                    if (Interlocked.Decrement(ref counter.ActiveItems) == 0)
                    {
                        workChannel.Writer.TryComplete();
                    }
                }
            }
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
            catch (Exception ex) when (ex is not OperationCanceledException) { }

            return new TotalScanValues() { TotalDirectoryCount = totalDirectoriesForThisLevel, TotalFileCount = totalFilesForThisLevel };
        }

        private async Task ProccessAndSendNodesAsync(ChannelReader<FileSystemNode> reader, IProgress<List<FileSystemNode>> progress, CancellationToken cancellationToken)
        {
            var buffer = new List<FileSystemNode>(200);

            try
            {
                await foreach (FileSystemNode node in reader.ReadAllAsync(cancellationToken))
                {
                    buffer.Add(node);

                    if (buffer.Count >= 200)
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
                UnicodeIcon = UnicodeManager.GetFileUnicodeByExtension(fileInfo.Extension),
                IconColor = ColorManager.GetFileIconColorByExtension(fileInfo.Extension),
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
    }
}
    