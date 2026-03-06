using FileSystem_Viewer.Services.IServices;
using Microsoft.UI.Dispatching;
using System;

namespace FileSystem_Viewer.Services
{
    public class DispatcherQueueProvider : IDispatcherQueueProvider
    {
        public DispatcherQueue DispatcherQueue { get; private set; } = null!;

        public void Initialize(DispatcherQueue dispatcherQueue)
        {
            DispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        }
    }
}
