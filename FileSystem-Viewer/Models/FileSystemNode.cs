using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.Data.Xml.Dom;

namespace FileSystem_Viewer.Models
{
    public abstract class FileSystemNode : ObservableObject
    {
        private BitmapImage? _icon;
        public BitmapImage? Icon
        {
            get { return _icon; }
            set { SetProperty(ref _icon, value); }
        }

        public bool IsDirectory => this is DirectoryNode;

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
            set { SetProperty(ref _size, value); }
        }

        private DateTime? _lastModified;
        public DateTime? LastModified
        {
            get { return _lastModified; }
            set { SetProperty(ref _lastModified, value); }
        }

        protected FileSystemNode() { }
        protected FileSystemNode(string name, string fullPath, long size, DateTime lastModified)
        {
            Name = name;
            FullPath = fullPath;
            Size = size;
            LastModified = lastModified;
        }
    }
}
