using System.Threading;
using System.Threading.Tasks;

namespace FileSystemViewer.Models
{
    public struct PauseResetToken
    {
        private PauseResetTokenSource _tokenSource;

        public PauseResetToken(PauseResetTokenSource pauseResetTokenSource)
        {
            _tokenSource = pauseResetTokenSource;
        }

        public Task IfPauseRequestedPauseAsync(CancellationToken cancellationToken)
        {
            if (_tokenSource == null)
                return Task.CompletedTask;

            return _tokenSource.WaitIfPausedAsync(cancellationToken);
        }
    }
}
