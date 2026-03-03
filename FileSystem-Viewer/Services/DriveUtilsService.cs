using FileSystem_Viewer.Models;
using FileSystem_Viewer.Services.IServices;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace FileSystem_Viewer.Services
{
    // Задачи:
    // - Дописываем сервис для Dispatcher +
    // - И лезем в систему оповещений UI чтобы динамически обновлять отображение
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

        public async Task ScanProvidedDrives(ObservableCollection<DirectoryNode> diskNodes, CancellationToken token)
        {
            if(diskNodes == null || !diskNodes.Any())
            {
                //
                return;
            }

            foreach (var directoryNode in diskNodes)
            {
                await Task.Run(async () =>
                {
                    //string drivePath = @$"\\.\{directoryNode.FullPath.TrimEnd('\\')}";

                    Stopwatch stopWatch = Stopwatch.StartNew();
                    await Scan(directoryNode, @"/", token, stopWatch);
                });
            }
        }

        public void ScanSpecifiedDirectory(string directory, ObservableCollection<FileSystemNode> fileSystemNodes)
        {
            throw new NotImplementedException();
        }

        private async Task Scan(DirectoryNode directoryNode, string directory, CancellationToken token, Stopwatch stopwatch)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                var directoryBuffer = new List<FileSystemNode>();
                long directorySize = 0;

                var currentDirectoryInfo = new DirectoryInfo(directory);

                try
                {
                    foreach (var fileInfo in currentDirectoryInfo.EnumerateFiles())
                    {
                        token.ThrowIfCancellationRequested();

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
                        catch (FileNotFoundException)
                        {
                            //
                        }
                        catch (Exception)
                        {
                            //
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    return;
                }
                catch (DirectoryNotFoundException) { return; }
                catch (Exception) { return; }

                SendDirectoryBuffer(directoryNode, directoryBuffer, ref directorySize, stopwatch);

                token.ThrowIfCancellationRequested();

                try
                {
                    foreach (var subDirectoryInfo in currentDirectoryInfo.EnumerateDirectories())
                    {
                        token.ThrowIfCancellationRequested();

                        DirectoryNode subDirectoryNode = new DirectoryNode(
                                name: subDirectoryInfo.Name,
                                fullPath: subDirectoryInfo.FullName,
                                size: 0,
                                lastModified: subDirectoryInfo.LastWriteTime);

                        directoryBuffer.Add(subDirectoryNode);

                        SendDirectoryBuffer(directoryNode, directoryBuffer, ref directorySize, stopwatch);

                        await Scan(subDirectoryNode, subDirectoryInfo.FullName, token, stopwatch);

                        DrivesUpdated?.Invoke(directoryNode, null, subDirectoryNode.Size);
                    }
                }
                catch (UnauthorizedAccessException) { /* Игнорируем закрытые системные папки */ }
                catch (Exception) { /* Игнорируем прочие ошибки I/O */ }
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception)
            {

            }
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
