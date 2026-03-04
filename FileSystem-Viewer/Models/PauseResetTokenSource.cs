using System.Threading;
using System.Threading.Tasks;

namespace FileSystem_Viewer.Models
{
    public class PauseResetTokenSource : IPauseResetTokenSource
    {
        private TaskCompletionSource<bool>? _pauseTcs;
        private readonly object _lock = new object();

        public PauseResetTokenSource()
        {
            Token = new PauseResetToken(this);
        }

        public bool IsPauseRequested { get; private set; }

        public PauseResetToken Token { get; set; }

        public void Pause()
        {
            lock (_lock)
            {
                if (!IsPauseRequested)
                {
                    IsPauseRequested = true;
                    _pauseTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                }
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                if (IsPauseRequested)
                {
                    IsPauseRequested = false;
                    _pauseTcs?.TrySetResult(true);
                }
            }
        }

        public Task WaitIfPausedAsync(CancellationToken cancellationToken)
        {
            Task awaitingTask;

            lock (_lock)
            {
                if (!IsPauseRequested || _pauseTcs == null)
                    return Task.CompletedTask;

                awaitingTask =  _pauseTcs.Task;
            }

            return awaitingTask.WaitAsync(cancellationToken); ;
        }
    }
}
