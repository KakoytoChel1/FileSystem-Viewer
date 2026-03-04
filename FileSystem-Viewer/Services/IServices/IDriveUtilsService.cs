using FileSystem_Viewer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileSystem_Viewer.Services.IServices
{
    public interface IDriveUtilsService
    {
        public event Action<DirectoryNode, List<FileSystemNode>?, long> DrivesUpdated;
        public List<DriveInfo> GetAvailableDrives();
        public Task ScanSpecifiedDirectory(DirectoryNode directoryNode, CancellationToken token, PauseResetToken pauseResetToken);
        public Task ScanProvidedDrives(ObservableCollection<DirectoryNode> fileSystemNodes, CancellationToken token, PauseResetToken pauseResetToken);
    }
}
