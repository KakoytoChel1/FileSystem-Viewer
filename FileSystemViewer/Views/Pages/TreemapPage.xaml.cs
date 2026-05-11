using FileSystemViewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace FileSystemViewer.Views.Pages
{
    public sealed partial class TreemapPage : Page
    {
        public MainPageViewModel MainPageViewModel { get; private set; } = null!;

        public TreemapPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var scopeProvider = e.Parameter as IServiceProvider;
            MainPageViewModel = scopeProvider!.GetRequiredService<MainPageViewModel>();
            base.OnNavigatedTo(e);
        }
    }
}
