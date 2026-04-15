using FileSystemViewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace FileSystemViewer.Views.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; private set; }
        private readonly TimeSpan _minimumTime = TimeSpan.FromMinutes(15);

        public SettingsPage()
        {
            InitializeComponent();
            ViewModel = (Application.Current as App)!.ServiceProvider.GetRequiredService<SettingsViewModel>();
        }

        private void DailyScanTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (e.NewTime < _minimumTime)
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    ViewModel.ScheduledScanTimeSpan = _minimumTime;
                });
            }
        }
    }
}
