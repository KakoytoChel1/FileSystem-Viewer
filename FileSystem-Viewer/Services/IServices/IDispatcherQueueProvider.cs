using Microsoft.UI.Dispatching;

namespace FileSystem_Viewer.Services.IServices
{
    public interface IDispatcherQueueProvider
    {
        public DispatcherQueue DispatcherQueue { get; }
        public void Initialize(DispatcherQueue dispatcherQueue);
    }
}
