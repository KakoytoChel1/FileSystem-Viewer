using FileSystem_Viewer.Models.DataModels;
using FileSystem_Viewer.ViewModels;
using FileSystemViewer.Services.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI;

namespace FileSystemViewer.Services
{
    public class FileExtentionItemService : IFileExtentionItemService
    {
        private Dictionary<string, FileExtensionItem> _fileExtensionsItems = new Dictionary<string, FileExtensionItem>();
        private Dictionary<string, Color> _extensionColorPairs;

        private readonly Color _defaultColor = Color.FromArgb(255, 128, 128, 128);

        public FileExtentionItemService()
        {
            _extensionColorPairs = new Dictionary<string, Color>()
            {
                {string.Empty, Color.FromArgb(255, 89, 94, 171) },

                {".jpg", Color.FromArgb(255, 56, 161, 199) },
                {".jpeg", Color.FromArgb(255, 56, 161, 199) },
                {".png", Color.FromArgb(255, 120, 56, 199) },
                {".gif", Color.FromArgb(255, 255, 215, 0) },
                {".svg", Color.FromArgb(255, 230, 76, 25) },
                {".psd", Color.FromArgb(255, 23, 19, 242) },

                {".mp4", Color.FromArgb(255, 184, 27, 227) },
                {".mov", Color.FromArgb(255, 165, 22, 204) },
                {".avi", Color.FromArgb(255, 169, 39, 204) },
                {".mkv", Color.FromArgb(255, 148, 6, 186) },

                {".mp3", Color.FromArgb(255, 21, 158, 171) },
                {".wav", Color.FromArgb(255, 7, 151, 173) },

                {".zip", Color.FromArgb(255, 148, 34, 34) },
                {".rar", Color.FromArgb(255, 143, 39, 39) },
                {".7z", Color.FromArgb(255, 156, 47, 47) },
                {".iso", Color.FromArgb(255, 188, 198, 204) },

                {".exe", Color.FromArgb(255, 39, 122, 72) },
                {".msi", Color.FromArgb(255, 57, 145, 54) },

                {".dll", Color.FromArgb(255, 101, 107, 112) },
                {".sys", Color.FromArgb(255, 90, 98, 105) },
                {".ini", Color.FromArgb(255, 76, 85, 92) },
                {".dat", Color.FromArgb(255, 20, 38, 140) }
            };
        }

        public void UpdateOrCreateFileExtensionItem(string extension, long size, long fileCount)
        {
            var properExtension = string.Empty;
            if (extension != null)
                properExtension = extension.ToLower();

            if (_fileExtensionsItems.TryGetValue(properExtension, out FileExtensionItem? item) && item != null)
            {
                item.FileCount += fileCount;
                item.Size += size;
            }
            else
            {
                FileExtensionItem fileExtensionItem = new FileExtensionItem()
                {
                    Extension = properExtension,
                    Size = size,
                    FileCount = fileCount
                };

                if (_extensionColorPairs.TryGetValue(fileExtensionItem.Extension, out Color color))
                    fileExtensionItem.Color = color;
                else
                    fileExtensionItem.Color = _defaultColor;

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
