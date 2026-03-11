using FileSystemViewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.IO;

namespace FileSystemViewer.Views.DialogPages;

public sealed partial class TargetSelectDialog : UserControl
{
    public MainPageViewModel ViewModel { get; private set; }

    public TargetSelectDialog()
    {
        InitializeComponent();

        ViewModel = (Application.Current as App)!.ServiceProvider.GetRequiredService<MainPageViewModel>();
    }

    private void availableDrives_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        foreach (DriveInfo item in e.AddedItems)
        {
            if (!ViewModel.SelectedTargetDrives.Contains(item))
            {
                ViewModel.SelectedTargetDrives.Add(item);
            }
        }

        foreach (DriveInfo item in e.RemovedItems)
        {
            if (ViewModel.SelectedTargetDrives.Contains(item))
            {
                ViewModel.SelectedTargetDrives.Remove(item);
            }
        }
    }
}
