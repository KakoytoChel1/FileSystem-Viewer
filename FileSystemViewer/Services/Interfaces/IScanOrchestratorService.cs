using FileSystemViewer.Models;
using FileSystemViewer.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace FileSystemViewer.Services.Interfaces
{
    public interface IScanOrchestratorService
    {
        Task<ScanResult> CoordinateScanningOperationAsync<T>(ObservableCollection<T> target, Action<List<FileSystemNode>> progressCallback, 
            CancellationTokenSource cts, PauseResetTokenSource prts) where T : DirectoryNode;
    }
}
