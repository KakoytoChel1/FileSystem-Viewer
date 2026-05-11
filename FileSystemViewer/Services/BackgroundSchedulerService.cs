using FileSystemViewer.Models;
using FileSystemViewer.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace FileSystemViewer.Services
{
    public class BackgroundSchedulerService : IBackgroundSchedulerService
    {
        public static readonly string TaskName = "FileViewerIntervalTask";

        public async Task<bool> RegisterIntervalTaskAsync(TimeSpan runTime)
        {
            try
            {
                var accessStatus = await BackgroundExecutionManager.RequestAccessAsync();

                if (accessStatus == BackgroundAccessStatus.DeniedByUser ||
                    accessStatus == BackgroundAccessStatus.DeniedBySystemPolicy)
                {
                    return false;
                }

                DeleteIntervalTask(TaskName);

                uint minutesToWait = (uint)(runTime.TotalMinutes);

                if (minutesToWait < 15) minutesToWait = 15;

                var builder = new Microsoft.Windows.ApplicationModel.Background.BackgroundTaskBuilder();
                builder.Name = TaskName;
                builder.SetTrigger(new TimeTrigger(minutesToWait, false));
                builder.SetTaskEntryPointClsid(typeof(DailyScanTask).GUID);
                builder.Register();

                return true;
            }
            catch (Exception)
            {
                // TODO: Log
                return false;
            }
        }

        public async Task<bool> UpdateIntervalTaskTimeAsync(TimeSpan newTime)
        {
            return await RegisterIntervalTaskAsync(newTime);
        }

        public bool DeleteIntervalTask(string? taskName)
        {
            if (string.IsNullOrWhiteSpace(taskName))
            {
                return false;
            }
            bool deleted = false;

            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == taskName)
                {
                    task.Value.Unregister(true);
                    deleted = true;
                }
            }

            return deleted;
        }
    }
}