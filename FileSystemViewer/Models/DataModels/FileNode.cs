using System;
using System.Diagnostics;
using System.IO;
using Windows.Storage;
using Windows.System;

namespace FileSystemViewer.Models
{
    public class FileNode(FileSystemNode parentNode) : FileSystemNode(parentNode)
    {
        public required string Extension { get; set; }

        protected async override void Open()
        {
            if (File.Exists(FullPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = FullPath,
                    UseShellExecute = true,
                });
            }
        }
    }
}
