using FileSystemViewer.Models;
using FileSystemViewer.Models.Tools;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Linq;
using System.Threading;

namespace FileSystemViewer
{
    public class Program
    {
        static private uint _RegistrationToken;
        static private ManualResetEvent _exitEvent = new ManualResetEvent(false);
        static void Main(string[] args)
        {
            if (args.Contains("-BackgroundTask"))
            {
                Guid taskGuid = typeof(DailyScanTask).GUID;
                ComServer.CoRegisterClassObject(ref taskGuid,
                                                new ComServer.BackgroundTaskFactory(),
                                                ComServer.CLSCTX_LOCAL_SERVER,
                                                ComServer.REGCLS_MULTIPLEUSE,
                                                out _RegistrationToken);

                _exitEvent.WaitOne();
            }
            else
            {
                Application.Start((p) =>
                {
                    var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                    SynchronizationContext.SetSynchronizationContext(context);
                    new App();
                });
            }
        }

        public static void SignalExit()
        {
            _exitEvent.Set();
        }
    }
}
