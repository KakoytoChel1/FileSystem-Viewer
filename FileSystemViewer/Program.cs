using FileSystemViewer.Models;
using FileSystemViewer.Models.Tools;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using System;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.Activation;

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
                try
                {
                    AppActivationArguments appActivationArguments;
                    bool isRedirectNeeded = DecideRedirectionNeed(out appActivationArguments);

                    if (!isRedirectNeeded)
                    {
                        Application.Start((p) =>
                        {
                            var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                            SynchronizationContext.SetSynchronizationContext(context);

                            var app = new App();

                            app.WindowCreated += () =>
                            {
                                RecognizeActivationKind(appActivationArguments, app);
                            };
                        });
                    }
                }  
                catch (Exception ex)
                {
                    Logger.Log($"Exception in main method: {ex}");
                }
            }
        }

        public static void SignalExit()
        {
            _exitEvent.Set();
        }

        private static bool DecideRedirectionNeed(out AppActivationArguments appActivationArguments)
        {
            bool isRedirectNeeded = false;

            AppActivationArguments args = AppInstance.GetCurrent().GetActivatedEventArgs();
            appActivationArguments = args;
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

        private static void OnActivated(object? sender, AppActivationArguments args)
        {
            if (Application.Current is App currentApp)
            {
                currentApp.HandleRedirectedActivation(args);
                RecognizeActivationKind(args, currentApp);
            }
        }

        private static void RecognizeActivationKind(AppActivationArguments args, App app)
        {
            try
            {
                switch (args.Data)
                {
                    case IFileActivatedEventArgs fileArgs:
                        app.HandleFileOpenActivation(fileArgs.Files);
                        break;

                    case IProtocolActivatedEventArgs protocolArgs:
                        app.HandleProtocolActivation(protocolArgs);
                        break;

                    case IStartupTaskActivatedEventArgs startupArgs:
                        app.HandleStartupActivation(startupArgs);
                        break;

                    case AppNotificationActivatedEventArgs notificationArgs:
                        app.HandleAppNotificationActivation(notificationArgs.Arguments);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Exception in RecognizeActivationKind: {ex}");
            }
        }
    }
}
