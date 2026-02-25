using FileSystem_Viewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FileSystem_Viewer.Views.Pages;

public sealed partial class MainPage : Page
{
    public MainPageViewModel ViewModel { get; private set; }

    public MainPage()
    {
        InitializeComponent();

        ViewModel = (Application.Current as App)!.ServiceProvider.GetRequiredService<MainPageViewModel>();
    }
}
