using System.Threading.Tasks;
using Microsoft.Windows.ApplicationModel.Resources;

namespace FileSystemViewer.Services.Interfaces
{
    public interface IBackgroundScannerService
    {
        public Task ProceedScan();
    }
}
