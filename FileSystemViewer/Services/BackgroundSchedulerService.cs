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
        public static readonly string TaskName = "FileViewerDailyTask";
        public static readonly string RecoveryTaskName = "FileViewerRecoveryScanTask";

        public async Task<bool> RegisterDailyTaskAsync(TimeSpan runTime)
        {
            try
            {
                var accessStatus = await BackgroundExecutionManager.RequestAccessAsync();

                if (accessStatus == BackgroundAccessStatus.DeniedByUser ||
                    accessStatus == BackgroundAccessStatus.DeniedBySystemPolicy)
                {
                    return false;
                }

                DeleteDailyTask(TaskName);

                // How much time left
                DateTime now = DateTime.Now;
                DateTime targetTime = now.Date + runTime;

                if (targetTime <= now)
                {
                    targetTime = targetTime.AddDays(1);
                }

                uint minutesToWait = (uint)(targetTime - now).TotalMinutes;
                if (minutesToWait < 15) minutesToWait = 15;

                var builder = new Microsoft.Windows.ApplicationModel.Background.BackgroundTaskBuilder();
                builder.Name = TaskName;
                builder.SetTrigger(new TimeTrigger(minutesToWait, true));

                builder.SetTaskEntryPointClsid(typeof(DailyScanTask).GUID);

                builder.Register();

                bool hasRecovery = BackgroundTaskRegistration.AllTasks.Any(t => t.Value.Name == RecoveryTaskName);

                if (!hasRecovery)
                {
                    var builderRecovery = new Microsoft.Windows.ApplicationModel.Background.BackgroundTaskBuilder();
                    builderRecovery.Name = RecoveryTaskName;
                    builderRecovery.SetTrigger(new SystemTrigger(SystemTriggerType.SessionConnected, false));

                    builderRecovery.SetTaskEntryPointClsid(typeof(DailyScanTask).GUID);
                    builderRecovery.Register();
                }
                return true;
            }
            catch (Exception)
            {
                // TODO: Log
                return false;
            }
        }

        public async Task<bool> UpdateDailyTaskTimeAsync(TimeSpan newTime)
        {
            return await RegisterDailyTaskAsync(newTime);
        }

        public bool DeleteDailyTask(string? taskName)
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