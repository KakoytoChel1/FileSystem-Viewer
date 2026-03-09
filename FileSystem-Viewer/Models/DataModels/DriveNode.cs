using System;
using System.Collections.Generic;

namespace FileSystem_Viewer.Models
{
    public class DriveNode : DirectoryNode
    {
        public DriveNode() : base(null) { }

        public DriveNode(string volumeName, long totalSize, long totalFreeSpace, string name, string fullPath, long size, DateTime lastModified) : base(null, name, fullPath, size, lastModified)
        {
            VolumeName = volumeName;
            TotalSize = totalSize;
            TotalFreeSpace = totalFreeSpace;
        }

        public DriveNode(string volumeName, long totalSize, long totalFreeSpace, string name, string fullPath, long size, DateTime lastModified, IEnumerable<FileSystemNode> fileSystemNodes) : base(null, name, fullPath, size, lastModified, fileSystemNodes)
        {
            VolumeName = volumeName;
            TotalSize = totalSize;
            TotalFreeSpace = totalFreeSpace;
        }

        private string? _volumeName;
        public string? VolumeName
        {
            get { return _volumeName; }
            set { SetProperty(ref _volumeName, value); }
        }

        private long _totalSize;
        public long TotalSize
        {
            get { return _totalSize; }
            set { SetProperty(ref _totalSize, value); }
        }

        private long _totalFreeSpace;
        public long TotalFreeSpace
        {
            get { return _totalFreeSpace; }
            set { SetProperty(ref _totalFreeSpace, value); }
        }
    }
}
