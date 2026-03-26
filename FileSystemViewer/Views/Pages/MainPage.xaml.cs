using FileSystemViewer.Models;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Specialized;
using System.Linq;

namespace FileSystemViewer.Views.Pages;

public sealed partial class MainPage : Page
{
    public MainPageViewModel ViewModel { get; private set; }

    public MainPage()
    {
        InitializeComponent();

        ViewModel = (Application.Current as App)!.ServiceProvider.GetRequiredService<MainPageViewModel>();


        foreach (DriveNode drive in ViewModel.DriveNodes)
        {
            AddDriveNode(drive);
        }

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

    private void AddDriveNode(DirectoryNode drive)
    {
        var node = new TreeViewNode
        {
            Content = drive,
            // There is sub collection in node or not
            HasUnrealizedChildren = true 
        };
        FileSystemTreeView.RootNodes.Add(node);
    }

    private void FileSystemTreeView_Expanding(TreeView sender, TreeViewExpandingEventArgs args)
    {
        if (args.Node.Content is DirectoryNode directoryNode)
        {
            directoryNode.IsExpanded = true;

            if (args.Node.Children.Count > 0)
                return;

            foreach (FileSystemNode childNode in directoryNode.FileSystemNodes)
            {
                AddFileSystemNode(args.Node, childNode);
            }

            if (directoryNode.FileSystemNodes is INotifyCollectionChanged observableCollection && !directoryNode.IsObserving)
            {
                directoryNode.IsObserving = true;
                observableCollection.CollectionChanged += (s, e) =>
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
        }
        else if (childModel is DirectoryNode dirNode)
        {
            treeViewNode.HasUnrealizedChildren = true;
        }

        parentNode.Children.Add(treeViewNode);
    }

    private void FileSystemTreeView_SelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args)
    {
        var selectedItems = sender.SelectedItems;
        ViewModel.FileSystemNodeSelectionChanged.Execute(selectedItems);
    }
}
