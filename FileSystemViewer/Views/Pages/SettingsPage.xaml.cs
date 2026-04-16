using FileSystemViewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FileSystemViewer.Views.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; private set; }

        public SettingsPage()
        {
            InitializeComponent();
            ViewModel = (Application.Current as App)!.ServiceProvider.GetRequiredService<SettingsViewModel>();
        }
    }
}
