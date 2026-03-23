using FileSystem_Viewer.Models.DataModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FileSystemViewer.Services.Interfaces
{
    public interface IFileExtentionItemService
    {
        public void UpdateOrCreateFileExtensionItem(string extension, long size, long fileCount);
        public List<FileExtensionItem> GetOrderedExtensionCollection();
        public void ClearFileExtensionCollection();
    }
}
