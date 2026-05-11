using System;
using System.Threading.Tasks;

namespace FileSystemViewer.Services.Interfaces
{
    public interface IBackgroundSchedulerService
    {
        public Task<bool> RegisterIntervalTaskAsync(TimeSpan runTime);
        public Task<bool> UpdateIntervalTaskTimeAsync(TimeSpan newTime);
        public bool DeleteIntervalTask(string taskName);
    }
}
