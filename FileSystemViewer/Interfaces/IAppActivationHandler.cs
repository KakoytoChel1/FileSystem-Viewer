using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace FileSystemViewer.Interfaces
{
    public interface IAppActivationHandler
    {
        public Task HandleCommandLineActivation(string[] arguments, bool isRedirected);
        public void HandleProtocolActivation(IProtocolActivatedEventArgs args);
        public void HandleFileOpenActivation(IReadOnlyList<IStorageItem> items);
        void HandleStartupActivation(IStartupTaskActivatedEventArgs args);
        void HandleAppNotificationActivation(IDictionary<string, string> arguments);
    }
}
