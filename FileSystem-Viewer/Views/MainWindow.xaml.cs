using FileSystem_Viewer.Views.Pages;
using Microsoft.UI.Xaml;

namespace FileSystem_Viewer
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;

            rootFrame.Navigate(typeof(MainPage));
        }
    }
}
