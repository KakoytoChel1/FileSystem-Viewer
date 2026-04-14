using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Services;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.Background;

namespace FileSystemViewer.Models
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("56DC28E7-8174-4A08-84CD-F3D4DF5B21F3")]
    [ComSourceInterfaces(typeof(IBackgroundTask))]
    public class DailyScanTask : IBackgroundTask
    {
        public IServiceProvider ServiceProvider { get; private set; } = null!;

        [MTAThread]
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += this.OnCanceled;
            try
            {
                InitializeServices();
                // Services 
                IBackgroundScannerService backgroundScannerService = ServiceProvider.GetRequiredService<IBackgroundScannerService>();
                IBackgroundSchedulerService backgroundSchedulerService = ServiceProvider.GetRequiredService<IBackgroundSchedulerService>();
                IConfigurationService<AppSettings> configurationService = ServiceProvider.GetRequiredService<IConfigurationService<AppSettings>>();

                // Check if background task exists in case we missed it
                bool isTaskScheduled = BackgroundTaskRegistration.AllTasks.Any(t => t.Value.Name == BackgroundSchedulerService.TaskName);
                if (!isTaskScheduled && configurationService.Settings.IsScheduledScanningEnabled)
                {
                    await backgroundSchedulerService.RegisterDailyTaskAsync(configurationService.Settings.ScheduledScanningTime);
                }

                await backgroundScannerService.ProceedScan();
                // Reset time for next day
                await backgroundSchedulerService.UpdateDailyTaskTimeAsync(configurationService.Settings.ScheduledScanningTime);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                deferral.Complete();
                //Program.SignalExit();
            }
        }

        [MTAThread]
        public void OnCanceled(IBackgroundTaskInstance taskInstance, BackgroundTaskCancellationReason cancellationReason)
        {
            
        }

        private void InitializeServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IDriveUtilsService, DriveUtilsService>();
            services.AddSingleton<IConfigurationService<AppSettings>, ConfigurationService<AppSettings>>();
            services.AddSingleton<IBackgroundScannerService, BackgroundScannerService>();
            services.AddSingleton<IBackgroundSchedulerService, BackgroundSchedulerService>();
            services.AddSingleton<IFileExtentionItemService, FileExtentionItemService>();

            services.AddSingleton(TimeProvider.System);

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}
