using FileSystem_Viewer.Models;
using FileSystem_Viewer.Services.IServices;
using FileSystem_Viewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace FileSystem_Viewer.Views.Pages;

public sealed partial class MainPage : Page
{
    public MainPageViewModel ViewModel { get; private set; }
    private IDispatcherQueueProvider dispatcherQueueProvider;

    public MainPage()
    {
        InitializeComponent();

        ViewModel = (Application.Current as App)!.ServiceProvider.GetRequiredService<MainPageViewModel>();
        dispatcherQueueProvider = (Application.Current as App)!.ServiceProvider.GetRequiredService<IDispatcherQueueProvider>();


        foreach (var drive in ViewModel.DriveNodes)
        {
            AddDriveNode(drive);
        }

        // Подписка на отслеживание изменений в главной коллекции
        ViewModel.DriveNodes.CollectionChanged += (s, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (DirectoryNode newDrive in e.NewItems)
                {
                    AddDriveNode(newDrive);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                treeView.RootNodes.Clear();
            }
        };
    }

    // Ручное добавление корневих элементов (дисков) главной коллекции в TreeView
    private void AddDriveNode(DirectoryNode drive)
    {
        var node = new TreeViewNode
        {
            Content = drive,
            HasUnrealizedChildren = true // Имеет ли или будет иметь в себе вложенные данные
        };
        treeView.RootNodes.Add(node);
    }

    private HashSet<TreeViewNode> _subscribedNodes = new HashSet<TreeViewNode>();

    // Когда определенный TreeViewItem разворачивается, запрашиваются его вложенные элементы и сразу рисуются.
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

            // Устанавливает подписку на изменение вложенной коллекции, только для тех кому еще не ставили
            if (dirNode.FileSystemNodes is INotifyCollectionChanged observable &&
            !_subscribedNodes.Contains(args.Node))
            {
                _subscribedNodes.Add(args.Node);
                observable.CollectionChanged += (s, e) =>
                {
                    if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
                    {
                        foreach (FileSystemNode newChild in e.NewItems)
                        {
                            AddFileSystemNode(args.Node, newChild);
                        }
                    }
                    else if (e.Action == NotifyCollectionChangedAction.Reset)
                    {
                        args.Node.Children.Clear();
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
        }

        parentNode.Children.Add(treeViewNode);
    }

    // Сигнализирует об изменении выбранного элемента в TreeView
    private void treeView_SelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args)
    {
        if (args.AddedItems.Count > 0)
            ViewModel.FileSystemNodeSelectionChanged.Execute((TreeViewNode)args.AddedItems.First());
        else if (args.RemovedItems.Count > 0)
            ViewModel.FileSystemNodeSelectionChanged.Execute(null);
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
