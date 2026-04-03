using Microsoft.UI.Dispatching;

namespace FileSystemViewer.Services.Interfaces
{
    public interface IDispatcherQueueProvider
    {
        public DispatcherQueue DispatcherQueue { get; }
        public void Initialize(DispatcherQueue dispatcherQueue);
    }
}
