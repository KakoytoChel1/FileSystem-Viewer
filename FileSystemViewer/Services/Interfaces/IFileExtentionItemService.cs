using FileSystemViewer.Models.DataModels;
using System.Collections.Generic;

namespace FileSystemViewer.Services.Interfaces
{
    public interface IFileExtentionItemService
    {
        public void UpdateOrCreateFileExtensionItem(string extension, long size, long fileCount);
        public List<FileExtensionItem> GetOrderedExtensionCollection();
        public void ClearFileExtensionCollection();
    }
}
