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
        public event Action<DirectoryNode, List<FileSystemNode>?, long, long> DrivesUpdated;
        public List<DriveInfo> GetAvailableDrives();
        public Task ScanSpecifiedDirectoryAsync(DirectoryNode directoryNode, CancellationToken token, PauseResetToken pauseResetToken);
        public Task ScanProvidedDrivesAsync(ObservableCollection<DriveNode> fileSystemNodes, IProgress<List<FileSystemNode>> progress, CancellationToken token, PauseResetToken pauseResetToken);
    }
}
