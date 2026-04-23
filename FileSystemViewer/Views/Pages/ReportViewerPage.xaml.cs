using FileSystemViewer.Models.DataModels;
using FileSystemViewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FileSystemViewer.Views.Pages
{
    public sealed partial class ReportViewerPage : Page
    {
        public ReportViewerPageViewModel? ViewModel { get; private set; }
        public ReportViewerPage()
        {
            InitializeComponent();
            ViewModel = (Application.Current as App)?.ServiceProvider.GetRequiredService<ReportViewerPageViewModel>();
        }

        private void ReportTabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            ViewModel!.CloseIndividualReportCommand.Execute(args.Item as ScanReport);
        }

        private void ReportTabView_AddTabButtonClick(TabView sender, object args)
        {
            ViewModel!.OpenReportCommand.Execute(this.XamlRoot);
        }
    }
}
