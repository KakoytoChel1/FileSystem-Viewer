using FileSystemViewer.Models.DataModels;
using FileSystemViewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace FileSystemViewer.Views.Pages
{
    public sealed partial class ReportViewerPage : Page
    {
        public ReportViewerPageViewModel ViewModel { get; private set; } = null!;
        public ReportViewerPage()
        {
            InitializeComponent();
            this.Unloaded += ReportViewerPage_Unloaded;
        }

        private void ReportViewerPage_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            this.Bindings.StopTracking();
            ViewModel = null!;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var scopeProvider = e.Parameter as IServiceProvider;
            ViewModel = scopeProvider!.GetRequiredService<ReportViewerPageViewModel>();
            base.OnNavigatedTo(e);
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
