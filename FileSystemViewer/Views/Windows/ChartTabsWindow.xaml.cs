using FileSystemViewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;

namespace FileSystemViewer.Views.Windows
{
    public sealed partial class ChartTabsWindow : Window
    {
        public ChartPageViewModel? ViewModel { get; }

        public ChartTabsWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;

            ViewModel = serviceProvider.GetRequiredService<ChartPageViewModel>();
        }
    }
}
