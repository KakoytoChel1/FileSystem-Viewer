using FileSystemViewer.Models;
using FileSystemViewer.Models.Tools;
using System;
using System.Linq;
using System.Threading;

namespace FileSystemViewer
{
    //public class Program
    //{
    //    static private uint _RegistrationToken;
    //    static private ManualResetEvent _exitEvent = new ManualResetEvent(false);

    //    [STAThread]
    //    static void Main(string[] args)
    //    {
    //        if (args.Contains("-BackgroundTask"))
    //        {
    //            WinRT.ComWrappersSupport.InitializeComWrappers();

    //            Guid taskGuid = typeof(DailyScanTask).GUID;
    //            ComServer.CoRegisterClassObject(ref taskGuid,
    //                                            new ComServer.BackgroundTaskFactory(),
    //                                            ComServer.CLSCTX_LOCAL_SERVER,
    //                                            ComServer.REGCLS_MULTIPLEUSE,
    //                                            out _RegistrationToken);

    //            _exitEvent.WaitOne();
    //        }
    //        else
    //        {
    //            App.Start(p => new App());
    //        }
    //    }

    //    public static void SignalExit()
    //    {
    //        _exitEvent.Set();
    //    }
    //}
}
