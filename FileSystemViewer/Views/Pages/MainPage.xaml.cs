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
        this.Unloaded += MainPage_Unloaded;
    }

    private void MainPage_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        this.Bindings.StopTracking();
        ViewModel = null!;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        var scopeProvider = e.Parameter as IServiceProvider;
        ViewModel = scopeProvider!.GetRequiredService<MainPageViewModel>();
        base.OnNavigatedTo(e);
    }
}
