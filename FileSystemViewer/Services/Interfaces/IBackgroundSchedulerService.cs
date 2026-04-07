using System;

namespace FileSystemViewer.Services.Interfaces
{
    public interface IBackgroundSchedulerService
    {
        public bool RegisterDailyTask(TimeSpan runTime);
        public bool UpdateDailyTaskTime(string taskName, TimeSpan newTime);
        public bool DeleteDailyTask(string taskName);
    }
}
