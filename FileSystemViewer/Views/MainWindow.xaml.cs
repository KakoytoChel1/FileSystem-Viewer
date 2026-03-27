using FileSystem_Viewer.Views.Pages;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using ModernControls.Models;
using System.Collections.ObjectModel;
using Windows.UI;

namespace FileSystemViewer
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;

            (Application.Current as App)?.ServiceProvider.GetRequiredService<IDispatcherQueueProvider>().Initialize(this.DispatcherQueue);

            RootFrame.Navigate(typeof(MainPage));
            ChartFrame.Navigate(typeof(ChartPage));

            // Test purposes :)
            ObservableCollection<TreemapNode> treemapNodes = new ObservableCollection<TreemapNode>()
            {
                new TreemapNode {LabeledName = "Hello World", Percent = 30, BackgroundColor = Color.FromArgb(255, 4, 153, 239)},
                new TreemapNode {LabeledName = "Testing", Percent = 20, BackgroundColor = Color.FromArgb(255, 96, 55, 179)},
                new TreemapNode {LabeledName = "ABC", Percent = 15, BackgroundColor = Color.FromArgb(255, 184, 51, 87)},
                new TreemapNode {LabeledName = "SAMURAI", Percent = 12, BackgroundColor = Color.FromArgb(255, 217, 79, 48)},

                new TreemapNode {LabeledName = "System32", Percent = 10, BackgroundColor = Color.FromArgb(255, 34, 177, 76)},
                new TreemapNode {LabeledName = "Downloads", Percent = 8, BackgroundColor = Color.FromArgb(255, 255, 165, 0)}, 
                new TreemapNode {LabeledName = "Documents", Percent = 3, BackgroundColor = Color.FromArgb(255, 112, 128, 144)},
                new TreemapNode {LabeledName = "Temp Files", Percent = 2, BackgroundColor = Color.FromArgb(255, 105, 105, 105)}
            };

            MyTreemap.ItemsSource = treemapNodes;
        }
    }
}
