using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Services.Interfaces;
using Microsoft.Win32.TaskScheduler;
using System;

namespace FileSystemViewer.Services
{
    public class BackgroundSchedulerService(IConfigurationService<AppSettings> configurationService) : IBackgroundSchedulerService
    {
        private IConfigurationService<AppSettings> _configurationService = configurationService;

        public static readonly string ArgumentName = "--run-background";

        public bool RegisterDailyTask(TimeSpan runTime)
        {
            string? exePath = Environment.ProcessPath;

            if (string.IsNullOrWhiteSpace(exePath))
            {
                return false;
            }

            using (TaskService taskService = new TaskService())
            {
                TaskDefinition taskDefinition = taskService.NewTask();
                taskDefinition.RegistrationInfo.Description = "Daily background scanning for FileSystemViewer application.";
                DailyTrigger dailyTrigger = new DailyTrigger
                {
                    StartBoundary = DateTime.Today + runTime,
                    DaysInterval = 1
                };
                taskDefinition.Triggers.Add(dailyTrigger);

                taskDefinition.Actions.Add(new ExecAction(exePath, ArgumentName, null));

                taskDefinition.Principal.RunLevel = TaskRunLevel.Highest;

                string taskPath = _configurationService.Settings.ScheduledScanningTaskPath;
                taskService.RootFolder.RegisterTaskDefinition(taskPath, taskDefinition);
                return true;
            }
        }

        public bool UpdateDailyTaskTime(string taskPath, TimeSpan newTime)
        {
            if (string.IsNullOrWhiteSpace(taskPath))
            {
                return false;
            }

            using (TaskService taskService = new TaskService())
            {
                Task? task = taskService.GetTask(taskPath);

                if (task == null)
                {
                    return false;
                }

                TaskDefinition taskDefinition = task.Definition;

                if (taskDefinition.Triggers.Count > 0 && taskDefinition.Triggers[0] is DailyTrigger dailyTrigger)
                {
                    dailyTrigger.StartBoundary = DateTime.Today + newTime;
                    taskService.RootFolder.RegisterTaskDefinition(taskPath, taskDefinition);
                    return true;
                }
            }
            return false;
        }

        public bool DeleteDailyTask(string? taskPath)
        {
            if (string.IsNullOrWhiteSpace(taskPath))
            {
                return false;
            }

            using (TaskService taskService = new TaskService())
            {
                Task? task = taskService.GetTask(taskPath);

                if (task == null)
                {
                    return false;
                }

                taskService.RootFolder.DeleteTask(taskPath);
                return true;
            }
        }  
    }
}
