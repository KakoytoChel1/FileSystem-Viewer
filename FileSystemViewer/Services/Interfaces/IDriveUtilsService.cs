using FileSystemViewer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileSystemViewer.Services.Interfaces
{
    public interface IDriveUtilsService
    {
        public List<DriveInfo> GetAvailableDrives();
        public Task ScanProvidedNodesAsync<T>(ObservableCollection<T> fileSystemNodes, IProgress<List<FileSystemNode>> progress, CancellationToken token, PauseResetToken pauseResetToken) where T : DirectoryNode;
    }
}
