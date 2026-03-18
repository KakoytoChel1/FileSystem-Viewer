using FileSystemViewer.Services.Interfaces;
using Microsoft.UI.Dispatching;
using System;

namespace FileSystemViewer.Services
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
