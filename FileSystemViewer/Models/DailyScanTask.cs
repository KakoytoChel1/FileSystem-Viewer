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
            try
            {
                InitializeServices();
                IBackgroundScannerService backgroundScannerService = ServiceProvider.GetRequiredService<IBackgroundScannerService>();

                await backgroundScannerService.ProceedScan();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                deferral.Complete();
                Program.SignalExit();
            }
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
