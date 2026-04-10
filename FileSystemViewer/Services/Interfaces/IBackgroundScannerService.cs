using System.Threading.Tasks;

namespace FileSystemViewer.Services.Interfaces
{
    public interface IBackgroundScannerService
    {
        public Task ProceedScan();
    }
}
