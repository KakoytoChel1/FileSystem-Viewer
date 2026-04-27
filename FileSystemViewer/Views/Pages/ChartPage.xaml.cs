using FileSystemViewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace FileSystemViewer.Views.Pages
{
    public sealed partial class ChartPage : Page
    {
        public ChartPageViewModel ViewModel { get; private set; } = null!;

        public ChartPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var scopeProvider = e.Parameter as IServiceProvider;
            ViewModel = scopeProvider!.GetRequiredService<ChartPageViewModel>();
            base.OnNavigatedTo(e);
        }
    }
}
