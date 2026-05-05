using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace FileSystemViewer.Views.Pages
{
    public sealed partial class ShellPage : Page
    {
        public ShellPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var scopeProvider = e.Parameter as IServiceProvider;

            MainPageFrame.Navigate(typeof(MainPage), scopeProvider);
            ChartPageFrame.Navigate(typeof(ChartPage), scopeProvider);
            TreemapPageFrame.Navigate(typeof(TreemapPage), scopeProvider);

            VerticalSplitter.DoubleTapped += VerticalSplitter_DoubleTapped;
            HorizontalSplitter.DoubleTapped += HorizontalSplitter_DoubleTapped;

            IDialogService dialogService = scopeProvider!.GetRequiredService<IDialogService>();
            dialogService.Initialize(this.XamlRoot);

            base.OnNavigatedTo(e);
        }

        private void LayoutStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState == null) return;

            switch (e.NewState.Name)
            {
                case "WideState":
                    FormWideState();
                    break;
                case "MiddleState":
                    FormMiddleState();
                    break;
                case "NarrowState":
                    FormNarrowState();
                    break;
            }
        }

        private void FormWideState()
        {
            TopRow.Height = new GridLength(1, GridUnitType.Star);
            MiddleRow.Height = new GridLength(1, GridUnitType.Auto);
            BottomRow.Height = new GridLength(0.4, GridUnitType.Star);

            LeftColumn.Width = new GridLength(1, GridUnitType.Star);
            MiddleColumn.Width = new GridLength(1, GridUnitType.Auto);
            RightColumn.Width = new GridLength(0.4, GridUnitType.Star);

            Grid.SetColumnSpan(TreemapBorder, 3);
            Grid.SetRowSpan(ChartBorder, 1);
            Grid.SetColumnSpan(HorizontalSplitter, 3);
        }

        private void FormMiddleState()
        {
            TopRow.Height = new GridLength(1, GridUnitType.Star);
            MiddleRow.Height = new GridLength(1, GridUnitType.Auto);
            BottomRow.Height = new GridLength(0);

            LeftColumn.Width = new GridLength(1, GridUnitType.Star);
            MiddleColumn.Width = new GridLength(1, GridUnitType.Auto);
            RightColumn.Width = new GridLength(0.6, GridUnitType.Star);

            Grid.SetColumnSpan(TreemapBorder, 1);
            Grid.SetRowSpan(ChartBorder, 3);
            Grid.SetColumnSpan(HorizontalSplitter, 2);
        }

        private void FormNarrowState()
        {
            TopRow.Height = new GridLength(1, GridUnitType.Star);
            MiddleRow.Height = new GridLength(1, GridUnitType.Auto);
            BottomRow.Height = new GridLength(0);

            LeftColumn.Width = new GridLength(1, GridUnitType.Star);
            MiddleColumn.Width = new GridLength(1, GridUnitType.Auto);
            RightColumn.Width = new GridLength(0);

            Grid.SetColumnSpan(TreemapBorder, 3);
            Grid.SetRowSpan(ChartBorder, 1);
            Grid.SetColumnSpan(HorizontalSplitter, 3);
        }

        private void HorizontalSplitter_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            BottomRow.Height = new GridLength(2);
            VisualStateManager.GoToState((Control)sender, "Normal", true);
        }

        private void VerticalSplitter_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            RightColumn.Width = new GridLength(2);
            VisualStateManager.GoToState((Control)sender, "Normal", true);
        }
    }
}
