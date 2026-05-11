using FileSystemViewer.ViewModels;
using FileSystemViewer.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;

namespace FileSystemViewer.Views.Windows
{
    public sealed partial class TreeViewWindow : Window
    {
        public MainPageViewModel? ViewModel { get; }

        public TreeViewWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            ViewModel = serviceProvider.GetRequiredService<MainPageViewModel>();

            mainPageFrame.Navigate(typeof(MainPage), serviceProvider);
        }
    }
}
