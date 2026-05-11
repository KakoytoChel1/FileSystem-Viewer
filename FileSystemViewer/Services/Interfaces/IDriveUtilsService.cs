using FileSystemViewer.Models;
using FileSystemViewer.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace FileSystemViewer.Services.Interfaces
{
    public interface IDriveUtilsService
    {
        public List<IDriveInfo> GetAvailableDrives();
        public Task ScanProvidedNodesAsync<T>(ObservableCollection<T> fileSystemNodes, Action<List<FileSystemNode>> progress, CancellationToken token, PauseResetToken pauseResetToken) where T : DirectoryNode;

        public TotalScanValues ScanDirectoryLevel(DirectoryNode directoryNode, string directoryPath);
        public DirectoryNode CreateDirectoryNode(DirectoryNode? parent, DirectoryInfo directoryInfo);
    }
}
