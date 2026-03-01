using DiscUtils.Ntfs;
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
    // Задачи:
    // - Дописываем сервис для Dispatcher +
    // - И лезем в систему оповещений UI чтобы динамически обновлять отображение
    public class DriveUtilsService : IDriveUtilsService
    {
        public event Action<DirectoryNode, List<FileSystemNode>?, long> DrivesUpdated = null!;

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
                await Task.Run(() =>
                {
                    string drivePath = @$"\\.\{directoryNode.FullPath.TrimEnd('\\')}";

                    using (FileStream fileStream = new FileStream(drivePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (NtfsFileSystem ntfsFileSystem = new NtfsFileSystem(fileStream))
                        {
                            Stopwatch stopWatch = Stopwatch.StartNew();
                            Scan(ntfsFileSystem, directoryNode, @"\", token, stopWatch);
                        }
                    }
                });
            }
        }

        public void ScanSpecifiedDirectory(string directory, ObservableCollection<FileSystemNode> fileSystemNodes)
        {
            throw new NotImplementedException();
        }

        private void Scan(NtfsFileSystem ntfsFileSystem, DirectoryNode directoryNode, string directory, CancellationToken token, Stopwatch stopwatch)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                var directoryBuffer = new List<FileSystemNode>();
                long directorySize = 0;

                foreach (var file in ntfsFileSystem.GetFiles(directory))
                {
                    token.ThrowIfCancellationRequested();

                    try
                    {
                        var fileInfo = ntfsFileSystem.GetFileInfo(file);

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

                SendDirectoryBuffer(directoryNode, directoryBuffer, ref directorySize, stopwatch);

                token.ThrowIfCancellationRequested();

                foreach (var subDirectory in ntfsFileSystem.GetDirectories(directory))
                {
                    token.ThrowIfCancellationRequested();

                    var subDirectoryInfo = ntfsFileSystem.GetDirectoryInfo(subDirectory);

                    DirectoryNode subDirectoryNode = new DirectoryNode(
                            name: subDirectoryInfo.Name,
                            fullPath: subDirectoryInfo.FullName,
                            size: 0,
                            lastModified: subDirectoryInfo.LastWriteTime);

                    directoryBuffer.Add(subDirectoryNode);

                    SendDirectoryBuffer(directoryNode, directoryBuffer, ref directorySize, stopwatch);

                    Scan(ntfsFileSystem, subDirectoryNode, subDirectory, token, stopwatch);

                    DrivesUpdated?.Invoke(directoryNode, null, subDirectoryNode.Size);
                }
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
