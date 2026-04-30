using FileSystemViewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace FileSystemViewer.Views.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel? ViewModel { get; private set; }

        public SettingsPage()
        {
            InitializeComponent();
            this.Unloaded += SettingsPage_Unloaded;
        }

        private void SettingsPage_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            this.Bindings.StopTracking();
            ViewModel = null!;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var scopeProvider = e.Parameter as IServiceProvider;
            ViewModel = scopeProvider!.GetRequiredService<SettingsViewModel>();
            base.OnNavigatedTo(e);
        }
    }
}
