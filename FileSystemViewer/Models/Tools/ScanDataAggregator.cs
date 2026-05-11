using FileSystemViewer.Models.DataModels;
using System;
using System.Collections.Generic;

namespace FileSystemViewer.Models.Tools
{
    public class ScanDataAggregator
    {
        public static void AggregateNodes(List<FileSystemNode> data, Action<FileNode>? onFileExtensionProcessed = null)
        {
            Dictionary<DirectoryNode, TotalScanValues> totalScanValues = new Dictionary<DirectoryNode, TotalScanValues>();

            foreach (FileSystemNode node in data)
            {
                DirectoryNode parentNode = (node.ParentNode as DirectoryNode)!;
                long fileSize = 0;
                long directoriesCount = 0;
                long fileCount = 0;

                parentNode.FileSystemNodes!.Add(node);

                if (node is DirectoryNode)
                {
                    directoriesCount++;
                }
                else if (node is FileNode fileNode)
                {
                    fileSize = fileNode.Size;
                    fileCount++;
                    onFileExtensionProcessed?.Invoke(fileNode);
                }

                if (totalScanValues.TryGetValue(parentNode, out var values))
                {
                    values.TotalSizeInBytes += fileSize;
                    values.TotalDirectoryCount += directoriesCount;
                    values.TotalFileCount += fileCount;
                }
                else
                {
                    values = new TotalScanValues
                    {
                        TotalSizeInBytes = fileSize,
                        TotalDirectoryCount = directoriesCount,
                        TotalFileCount = fileCount
                    };
                    totalScanValues.Add(parentNode, values);
                }
            }

            foreach (KeyValuePair<DirectoryNode, TotalScanValues> pair in totalScanValues)
            {
                var current = pair.Key;

                while (current != null)
                {
                    current.Size += pair.Value.TotalSizeInBytes;
                    current.FileCount += pair.Value.TotalFileCount;
                    current.DirectoriesCount += pair.Value.TotalDirectoryCount;
                    current = current.ParentNode as DirectoryNode;
                }
            }
        }
    }
}
