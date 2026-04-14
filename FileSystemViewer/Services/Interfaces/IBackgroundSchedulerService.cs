using System;
using System.Threading.Tasks;

namespace FileSystemViewer.Services.Interfaces
{
    public interface IBackgroundSchedulerService
    {
        public Task<bool> RegisterDailyTaskAsync(TimeSpan runTime);
        public Task<bool> UpdateDailyTaskTimeAsync(TimeSpan newTime);
        public bool DeleteDailyTask(string taskName);
    }
}
