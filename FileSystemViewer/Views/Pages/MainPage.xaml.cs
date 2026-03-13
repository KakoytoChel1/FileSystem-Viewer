using FileSystemViewer.Models;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace FileSystemViewer.Views.Pages;

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
                FileSystemTreeView.RootNodes.Clear();
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
        FileSystemTreeView.RootNodes.Add(node);
    }

    private HashSet<TreeViewNode> _subscribedNodes = new HashSet<TreeViewNode>();

    // Когда определенный TreeViewItem разворачивается, запрашиваются его вложенные элементы и сразу рисуются.
    private void FileSystemTreeView_Expanding(TreeView sender, TreeViewExpandingEventArgs args)
    {
        if (args.Node.Content is DirectoryNode dirNode)
        {
            dirNode.IsExpanded = true;

            if (args.Node.Children.ToList().Any())
                return;

            foreach (FileSystemNode childNode in dirNode.FileSystemNodes)
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

    private void FileSystemTreeView_Collapsed(TreeView sender, TreeViewCollapsedEventArgs args)
    {
        if (args.Node.Content is DirectoryNode dirNode)
        {
            dirNode.IsExpanded = false;
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
    private void FileSystemTreeView_SelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args)
    {
        if (args.AddedItems.Count > 0)
            ViewModel.FileSystemNodeSelectionChanged.Execute(args.AddedItems);
        else if (args.RemovedItems.Count > 0)
            ViewModel.FileSystemNodeSelectionChanged.Execute(null);
    }
}
