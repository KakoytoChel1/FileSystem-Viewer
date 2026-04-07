using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Models;
using FileSystemViewer.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;

namespace FileSystemViewer.Services
{
    public class FileExtentionItemService : IFileExtentionItemService
    {
        private Dictionary<string, FileExtensionItem> _fileExtensionsItems = new Dictionary<string, FileExtensionItem>();

        public void UpdateOrCreateFileExtensionItem(string extension, long size, long fileCount)
        {
            var properExtension = string.Empty;
            if (extension != null)
            {
                properExtension = extension.ToLower();
            }

            if (_fileExtensionsItems.TryGetValue(properExtension, out FileExtensionItem? item) && item != null)
            {
                item.FileCount += fileCount;
                item.Size += size;
            }
            else
            {
                Color fileExtensionColor;

                if (ColorManager.ExtensionColorPairs.TryGetValue(properExtension, out Color color))
                {
                    fileExtensionColor = color;
                }
                else
                {
                    fileExtensionColor = ColorManager.DefaultColor;
                }

                FileExtensionItem fileExtensionItem = new FileExtensionItem()
                {
                    Extension = properExtension,
                    Size = size,
                    FileCount = fileCount,
                    Color = fileExtensionColor
                };

                _fileExtensionsItems.Add(fileExtensionItem.Extension, fileExtensionItem);
            }
        }

        public List<FileExtensionItem> GetOrderedExtensionCollection()
        {
            if (_fileExtensionsItems.Count > 0)
            {
                return _fileExtensionsItems.Values.OrderByDescending(v => v.Size).ToList();
            }
            return new List<FileExtensionItem>();
        }

        public void ClearFileExtensionCollection()
        {
            _fileExtensionsItems.Clear();
        }
    }
}
