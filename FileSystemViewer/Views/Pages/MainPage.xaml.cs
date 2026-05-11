using FileSystemViewer.Models;
using FileSystemViewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace FileSystemViewer.Views.Pages;

public sealed partial class MainPage : Page
{
    public MainPageViewModel ViewModel { get; private set; } = null!;

    public MainPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        var scopeProvider = e.Parameter as IServiceProvider;
        ViewModel = scopeProvider!.GetRequiredService<MainPageViewModel>();
        base.OnNavigatedTo(e);
    }

    private void OpenBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem menuFlyoutItem)
        {
            if (menuFlyoutItem.DataContext is FileSystemNode fileSystemNode)
            {
                ViewModel.OpenFileSystemNodeInExplorerCommand.Execute(fileSystemNode);
            }
        }
    }

    private void CopyPathBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem menuFlyoutItem)
        {
            if (menuFlyoutItem.DataContext is FileSystemNode fileSystemNode)
            {
                ViewModel.CopyFileSystemNodePathCommand.Execute(fileSystemNode);
            }
        }
    }
}
