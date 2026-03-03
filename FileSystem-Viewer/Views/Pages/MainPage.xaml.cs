using FileSystem_Viewer.Models;
using FileSystem_Viewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace FileSystem_Viewer.Views.Pages;

public sealed partial class MainPage : Page
{
    public MainPageViewModel ViewModel { get; private set; }

    public MainPage()
    {
        InitializeComponent();

        ViewModel = (Application.Current as App)!.ServiceProvider.GetRequiredService<MainPageViewModel>();

        //ViewModel.DriveNodes.CollectionChanged += (s, e) =>
        //{
        //    if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
        //    {
        //        foreach (DirectoryNode newDrive in e.NewItems)
        //        {
        //            TreeViewNode treeViewNode = new TreeViewNode { Content = newDrive, HasUnrealizedChildren = true };
        //            BitmapImage bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/drive.png"));
        //            newDrive.Icon = bitmapImage;
        //            treeView.RootNodes.Add(treeViewNode);
        //        }
        //    }
        //};
    }

    private void treeView_Expanding(TreeView sender, TreeViewExpandingEventArgs args)
    {
        if (args.Node.Content is DirectoryNode dirNode)
        {
            if (args.Node.Children.ToList().Any())
                return;

            foreach (var childNode in dirNode.FileSystemNodes)
            {
                AddFileSystemNode(args.Node, childNode);
            }

            if (dirNode.FileSystemNodes is INotifyCollectionChanged observableList)
            {
                observableList.CollectionChanged += async (s, e) =>
                {
                    if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
                    {
                        foreach (FileSystemNode newChild in e.NewItems)
                        {
                            AddFileSystemNode(args.Node, newChild);
                        }
                    }
                };
            }
        }
    }

    private void AddFileSystemNode(TreeViewNode parentNode, FileSystemNode childModel)
    {
        TreeViewNode treeViewNode = new TreeViewNode { Content = childModel };

        if (childModel is FileNode fileNode)
        {
            treeViewNode.HasUnrealizedChildren = false;
            BitmapImage bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/file.png"));
            childModel.Icon = bitmapImage;
        }
        else if (childModel is DirectoryNode dirNode)
        {
            treeViewNode.HasUnrealizedChildren = true;
            BitmapImage bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/directory.png"));
            childModel.Icon = bitmapImage;
        }

        parentNode.Children.Add(treeViewNode);
    }

    private async Task<BitmapImage?> GetFileIconAsync(string filePath)
    {
        try
        {
            StorageFolder sf = await StorageFolder.GetFolderFromPathAsync(filePath);
            StorageItemThumbnail thumbnail = await sf.GetThumbnailAsync(ThumbnailMode.SingleItem, 32, ThumbnailOptions.UseCurrentScale);

            if (thumbnail != null)
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(thumbnail);
                return bitmapImage;
            }
        }
        catch (Exception)
        {
            return null;
        }

        return null;
    }
}
