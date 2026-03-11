using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

namespace FileSystemViewer.Models
{
    public abstract class FileSystemNode : ObservableObject
    {
        private BitmapImage? _icon;
        public BitmapImage? Icon
        {
            get { return _icon; }
            set { SetProperty(ref _icon, value); }
        }

        private string _name = null!;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private string _fullPath = null!;
        public string FullPath
        {
            get { return _fullPath; }
            set { SetProperty(ref _fullPath, value); }
        }

        private long _size;
        public long Size
        {
            get { return _size; }
            set 
            {
                SetProperty(ref _size, value);
            }
        }

        private DateTime? _lastModified;
        public DateTime? LastModified
        {
            get { return _lastModified; }
            set { SetProperty(ref _lastModified, value); }
        }

        private FileSystemNode? _parentNode;
        public FileSystemNode? ParentNode
        {
            get { return _parentNode; }
            private set { SetProperty(ref _parentNode, value); }
        }

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

        public void UpdatePercentForUI()
        {
            OnPropertyChanged(nameof(PercentProperty));
        }

        protected FileSystemNode(FileSystemNode? parentNode)
        {
            ParentNode = parentNode;
        }

        protected FileSystemNode(FileSystemNode? parentNode, string name, string fullPath, long size, DateTime lastModified)
        {
            Name = name;
            FullPath = fullPath;
            Size = size;
            LastModified = lastModified;
            ParentNode = parentNode;
        }
    }
}
