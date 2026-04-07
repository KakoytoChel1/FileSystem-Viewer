using FileSystemViewer.Services.Interfaces;
using Microsoft.Win32.TaskScheduler;
using System;

namespace FileSystemViewer.Services
{
    public class BackgroundSchedulerService : IBackgroundSchedulerService
    {
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
                taskDefinition.RegistrationInfo.Description = "Daily background scanning for File System Viewer application.";
                DailyTrigger dailyTrigger = new DailyTrigger
                {
                    StartBoundary = DateTime.Today + runTime,
                    DaysInterval = 1
                };
                taskDefinition.Triggers.Add(dailyTrigger);

                taskDefinition.Actions.Add(new ExecAction(exePath, "--run-background", null));

                taskDefinition.Principal.RunLevel = TaskRunLevel.Highest;

                taskService.RootFolder.RegisterTaskDefinition(@"FileSystemViewer", taskDefinition);
                return true;
            }
        }

        public bool UpdateDailyTaskTime(string taskName, TimeSpan newTime)
        {
            if (string.IsNullOrWhiteSpace(taskName))
            {
                return false;
            }

            using (TaskService taskService = new TaskService())
            {
                Task? task = taskService.GetTask(taskName);

                if (task == null)
                {
                    return false;
                }

                TaskDefinition taskDefinition = task.Definition;

                if (taskDefinition.Triggers.Count > 0 && taskDefinition.Triggers[0] is DailyTrigger dailyTrigger)
                {
                    dailyTrigger.StartBoundary = DateTime.Today + newTime;
                    taskService.RootFolder.RegisterTaskDefinition(taskName, taskDefinition);
                    return true;
                }
            }
            return false;
        }

        public bool DeleteDailyTask(string? taskName)
        {
            if (string.IsNullOrWhiteSpace(taskName))
            {
                return false;
            }

            using (TaskService taskService = new TaskService())
            {
                Task? task = taskService.GetTask(taskName);

                if (task == null)
                {
                    return false;
                }

                taskService.RootFolder.DeleteTask(taskName);
                return true;
            }
        }  
    }
}
