using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.UI;

namespace FileSystemViewer.Models
{
    public abstract class FileSystemNode : ObservableObject
    {
        public FileSystemNode(FileSystemNode? parentNode)
        {
            ParentNode = parentNode;
        }

        public required string UnicodeIcon { get; set; }
        public required Color IconColor { get; set; }
        public required string Name { get; set; }
        public required string FullPath { get; set; }
        public required long Size { get; set; }
        public required DateTime? LastModified { get; set; }
        public FileSystemNode? ParentNode { get; private set; }

        public double PercentProperty
        {
            get
            {
                if (ParentNode == null) return 100;

                if (ParentNode.Size == 0) return 0;

                double result = (double)Size / ParentNode.Size;
                return result * 100;
            }
        }
        public long FileCount { get; set;  }

        public virtual ObservableCollection<FileSystemNode>? FileSystemNodes { get; set; } = null;

        public virtual bool? IsExpanded { get; set; } = null;

        public virtual bool IsInProgress { get; set; } = false;

        public virtual string Tag { get; set; } = string.Empty;

        public void UpdatePercentProperty()
        {
            OnPropertyChanged(nameof(PercentProperty));
        }

        public void UpdateSizeProperty()
        {
            OnPropertyChanged(nameof(Size));
        }
    }
}
