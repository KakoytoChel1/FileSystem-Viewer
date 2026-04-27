using FileSystemViewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;

namespace FileSystemViewer.Views.Windows
{
    public sealed partial class TreemapWindow : Window
    {
        public MainPageViewModel? ViewModel { get; }

        public TreemapWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            ViewModel = serviceProvider.GetRequiredService<MainPageViewModel>();
        }
    }
}
