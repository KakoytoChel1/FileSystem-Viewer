using FileSystemViewer.Models;
using FileSystemViewer.Models.Tools;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileSystemViewer
{
    public class Program
    {
        static private uint _RegistrationToken;
        static private ManualResetEvent _exitEvent = new ManualResetEvent(false);

        [STAThread]
        static void Main(string[] args)
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();

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
                bool isRedirectNeeded = DecideRedirectionNeed();

                if (!isRedirectNeeded)
                {
                    Application.Start((p) =>
                    {
                        var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                        SynchronizationContext.SetSynchronizationContext(context);
                        new App();
                    });
                }   
            }
        }

        public static void SignalExit()
        {
            _exitEvent.Set();
        }

        private static bool DecideRedirectionNeed()
        {
            bool isRedirectNeeded = false;

            AppActivationArguments args = AppInstance.GetCurrent().GetActivatedEventArgs();
            AppInstance keyInstance = AppInstance.FindOrRegisterForKey("FileSystemViewer");

            // Is our keyInstance was created in current proccess
            if (keyInstance.IsCurrent)
            {
                keyInstance.Activated += OnActivated;
            }
            else
            {
                isRedirectNeeded = true;
                keyInstance.RedirectActivationToAsync(args).AsTask().Wait();
            }

            return isRedirectNeeded;
        }

        private static void OnActivated(object? sender, AppActivationArguments e)
        {
            if (Application.Current is App currentApp)
            {
                currentApp.HandleActivation(e);
            }
        }
    }
}
