using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileSystemViewer.Models;
using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Models.Tools;
using FileSystemViewer.Services.Interfaces;
using LiveChartsCore;
using Microsoft.UI.Xaml;
using ModernControls.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileSystemViewer.ViewModels
{
    public partial class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel(IServiceProvider serviceProvider, IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider,
            IFileExtentionItemService fileExtentionItemService, IConfigurationService<AppSettings> configurationService, IVisualManagerService visualManagerService, AppState appState,
            TimeProvider timeProvider, INotificationService notificationService, IReportStorageService reportStorageService, IDialogService dialogService, ISubWindowManagerService subWindowManagerService) : base(serviceProvider, driveUtilsService,
                dispatcherQueueProvider, fileExtentionItemService, configurationService, visualManagerService, notificationService, reportStorageService, dialogService, appState)
        {
            DriveNodes = new ObservableCollection<DirectoryNode>();
            AllAvailableDrives = new ObservableCollection<DriveInfo>();
            SelectedTargetDrives = new ObservableCollection<DriveInfo>();
            TreemapNodes = new ObservableCollection<TreemapNode>();
            _subWindowManagerService = subWindowManagerService;

            TimeProvider = timeProvider;

            SelectedScanningTargetIndex = 0;
            ApplicationState.CurrentScanningState = AppState.ScanningStates.None;

            ApplicationState.ScanningStatePropertyChanged += ApplicationState_ScanningStatePropertyChanged;
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                ApplicationState.ScanningStatePropertyChanged -= ApplicationState_ScanningStatePropertyChanged;
                CurrentScanningCancellationTokenSource?.Dispose();
                SelectedFileSystemNode = null;

                if (DriveNodes != null)
                {
                    foreach (var drive in DriveNodes)
                    {
                        DestroyFileSystemNodesRecursively(drive);
                    }
                    DriveNodes.Clear();
                    DriveNodes = null!;
                }

                if (TreemapNodes != null)
                {
                    foreach (var treemapNode in TreemapNodes)
                    {
                        DestroyTreemapNodeRecursively(treemapNode);
                    }
                    TreemapNodes.Clear();
                    TreemapNodes = null!;
                }

                base.Dispose();
            }
        }

        private void DestroyFileSystemNodesRecursively(FileSystemNode node)
        {
            if (node == null || node.FileSystemNodes == null)
                return;

            var children = node.FileSystemNodes;
            node.FileSystemNodes = null;

            foreach (var child in children)
            {
                DestroyFileSystemNodesRecursively(child);
            }
            children.Clear();
        }

        private void DestroyTreemapNodeRecursively(TreemapNode node)
        {
            if (node == null || node.Children == null)
                return;

            var children = node.Children;
            node.Children = null;

            foreach (var child in children)
            {
                DestroyTreemapNodeRecursively(child);
            }
            children.Clear();
        }

        private void ApplicationState_ScanningStatePropertyChanged()
        {
            OpenTargetSelectDialogCommand.NotifyCanExecuteChanged();
            RefreshScanningCommand.NotifyCanExecuteChanged();
            RescanSelectedDirectoriesCommand.NotifyCanExecuteChanged();
            CancelScanningCommand.NotifyCanExecuteChanged();
            ResumeScanningCommand.NotifyCanExecuteChanged();
            PauseScanningCommand.NotifyCanExecuteChanged();
        }

        private ISubWindowManagerService _subWindowManagerService;
        TimeProvider TimeProvider { get; }
        private CancellationTokenSource? CurrentScanningCancellationTokenSource { get; set; }
        private PauseResetTokenSource? PauseResetTokenSource { get; set; }

        /// <summary>
        /// Main nodes collection, contains all selected drives with their inner collections.
        /// </summary>
        public ObservableCollection<DirectoryNode> DriveNodes { get; set; }
        public ObservableCollection<DriveInfo> AllAvailableDrives { get; set; }
        public ObservableCollection<DriveInfo> SelectedTargetDrives { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<TreemapNode> TreemapNodes { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RescanSelectedDirectoriesCommand))]
        public partial FileSystemNode? SelectedFileSystemNode { get; set; }

        // Selection mode: all drives (0) or selected (1).
        [ObservableProperty]
        public partial int SelectedScanningTargetIndex { get; set; }

        [ObservableProperty]
        public partial Visibility ProgressBarVisibility { get; set; }

        [RelayCommand(CanExecute = nameof(IsScanningOperationsAvailable))]
        public async Task OpenTargetSelectDialog()
        {
            LoadAvailableDrives();
            SelectedTargetDrives.Clear();

            if (await DialogService.ShowTargetSelectionDialogAsync(ServiceProvider!))
            {
                if (SelectedScanningTargetIndex == 1)
                {
                    if (!SelectedTargetDrives.Any()) { return; }

                    SelectedFileSystemNode = null;
                    DriveNodes.Clear();

                    if (CurrentScanningCancellationTokenSource != null)
                    {
                        CurrentScanningCancellationTokenSource.Dispose();
                    }

                    CurrentScanningCancellationTokenSource = new CancellationTokenSource();
                    PauseResetTokenSource = new PauseResetTokenSource();

                    foreach (DriveInfo driveInfo in SelectedTargetDrives)
                    {
                        var name = !string.IsNullOrWhiteSpace(driveInfo.VolumeLabel) ? $"{driveInfo.VolumeLabel} {driveInfo.Name}" : driveInfo.Name;

                        DriveNode drive = new DriveNode()
                        {
                            Name = name,
                            FullPath = driveInfo.RootDirectory.FullName,
                            Size = 0,
                            LastModified = driveInfo.RootDirectory.LastWriteTime,
                            UnicodeIcon = UnicodeManager.DriveIcon,
                            IconColor = ColorManager.DriveIconColor,

                            VolumeName = driveInfo.VolumeLabel,
                            TotalFreeSpace = driveInfo.TotalFreeSpace,
                            TotalSize = driveInfo.TotalSize
                        };
                        DriveNodes.Add(drive);
                    }
                    await ProceedScanForSelectedTargetAsync(DriveNodes, CurrentScanningCancellationTokenSource, PauseResetTokenSource);
                }
                else
                {
                    SelectedFileSystemNode = null;
                    DriveNodes.Clear();
                    CurrentScanningCancellationTokenSource = new CancellationTokenSource();
                    PauseResetTokenSource = new PauseResetTokenSource();

                    foreach (DriveInfo driveInfo in AllAvailableDrives)
                    {
                        var name = !string.IsNullOrWhiteSpace(driveInfo.VolumeLabel) ? $"{driveInfo.VolumeLabel} {driveInfo.Name}" : driveInfo.Name;

                        DriveNode drive = new DriveNode()
                        {
                            Name = name,
                            FullPath = driveInfo.RootDirectory.FullName,
                            Size = 0,
                            LastModified = driveInfo.RootDirectory.LastWriteTime,
                            UnicodeIcon = UnicodeManager.DriveIcon,
                            IconColor = ColorManager.DriveIconColor,

                            VolumeName = driveInfo.VolumeLabel,
                            TotalFreeSpace = driveInfo.TotalFreeSpace,
                            TotalSize = driveInfo.TotalSize
                        };
                        DriveNodes.Add(drive);
                    }
                    await ProceedScanForSelectedTargetAsync(DriveNodes, CurrentScanningCancellationTokenSource, PauseResetTokenSource);
                }
            }
        }
        private bool IsScanningOperationsAvailable() => ApplicationState.CurrentScanningState == AppState.ScanningStates.None || ApplicationState.CurrentScanningState == AppState.ScanningStates.Completed || ApplicationState.CurrentScanningState == AppState.ScanningStates.Canceled;

        // Updates drives list in selection target menu.
        [RelayCommand]
        public void RefreshAvailableDrivesCollection()
        {
            LoadAvailableDrives();
        }

        // Starts scanning target again.
        [RelayCommand(CanExecute = nameof(IsScanningOperationsAvailable))]
        public async Task RefreshScanning()
        {
            if (!DriveNodes.Any())
                return;

            if (await DialogService.ConfirmRefreshScanningAsync(DriveNodes.Count))
            {
                foreach (DirectoryNode driveNode in DriveNodes)
                {
                    driveNode.FileSystemNodes!.Clear();
                    driveNode.FileCount = 0;
                    driveNode.Size = 0;
                    driveNode.IsExpanded = false;

                    driveNode.UpdateSizeProperty();
                    driveNode.UpdateFileCountProperty();
                }

                if (CurrentScanningCancellationTokenSource != null)
                {
                    CurrentScanningCancellationTokenSource.Dispose();
                }

                CurrentScanningCancellationTokenSource = new CancellationTokenSource();
                PauseResetTokenSource = new PauseResetTokenSource();

                await ProceedScanForSelectedTargetAsync(DriveNodes, CurrentScanningCancellationTokenSource, PauseResetTokenSource);
            }
        }

        // Starts scanning target again for selected directory nodes.
        [RelayCommand(CanExecute = nameof(IsDirectoryScanningAvailable))]
        public async Task RescanSelectedDirectories()
        {
            if (SelectedFileSystemNode != null && SelectedFileSystemNode is DirectoryNode selectedDirectoryNode)
            {
                if (await DialogService.ConfirmSelectedDirectoryScanningAsync())
                {
                    if (CurrentScanningCancellationTokenSource != null)
                    {
                        CurrentScanningCancellationTokenSource.Dispose();
                    }

                    CurrentScanningCancellationTokenSource = new CancellationTokenSource();
                    PauseResetTokenSource = new PauseResetTokenSource();

                    RemoveDataForRescannedDirectory(selectedDirectoryNode);

                    selectedDirectoryNode.FileSystemNodes!.Clear();
                    selectedDirectoryNode.FileCount = 0;
                    selectedDirectoryNode.Size = 0;

                    await ProceedScanForSelectedTargetAsync(new ObservableCollection<DirectoryNode>() { selectedDirectoryNode }, CurrentScanningCancellationTokenSource, PauseResetTokenSource);
                }
            }
        }
        private bool IsDirectoryScanningAvailable() => (ApplicationState.CurrentScanningState == AppState.ScanningStates.Completed || ApplicationState.CurrentScanningState == AppState.ScanningStates.Canceled) && (SelectedFileSystemNode != null && SelectedFileSystemNode is DirectoryNode);

        [RelayCommand(CanExecute = nameof(IsCancelScanningAvailable))]
        public async Task CancelScanning()
        {
            if (await DialogService.ConfirmScanningCancellingAsync())
            {
                CurrentScanningCancellationTokenSource!.Cancel();
                ApplicationState.CurrentScanningState = AppState.ScanningStates.Canceled;
            }
        }
        private bool IsCancelScanningAvailable() => ApplicationState.CurrentScanningState == AppState.ScanningStates.InProgress || ApplicationState.CurrentScanningState == AppState.ScanningStates.Paused;


        [RelayCommand(CanExecute = nameof(IsResumeScanningAvailable))]
        public void ResumeScanning()
        {
            PauseResetTokenSource!.Reset();
            ApplicationState.CurrentScanningState = AppState.ScanningStates.InProgress;

        }
        private bool IsResumeScanningAvailable() => ApplicationState.CurrentScanningState == AppState.ScanningStates.Paused;

        [RelayCommand(CanExecute = nameof(IsPauseScanningAvailable))]
        public async Task PauseScanning()
        {
            PauseResetTokenSource!.Pause();
            ApplicationState.CurrentScanningState = AppState.ScanningStates.Paused;

        }
        private bool IsPauseScanningAvailable() => ApplicationState.CurrentScanningState == AppState.ScanningStates.InProgress;

        [RelayCommand]
        public void OpenTreeViewNewWindow()
        {
            _subWindowManagerService.OpenTreeViewSubWindow();
        }

        [RelayCommand]
        public void OpenTreeMapNewWindow()
        {
            _subWindowManagerService.OpenTreemapSubWindow();
        }

        [RelayCommand]
        public void OpenSettingsMenu()
        {
            ApplicationState.SettingsMenuVisibility = Visibility.Visible;
        }

        [RelayCommand]
        public void OpenTreemapDirectory(object parameter)
        {
            if (parameter is TreemapNode treemapNode)
            {
                if (Directory.Exists(treemapNode.FullPath))
                {

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = treemapNode.FullPath,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }
            }
        }

        public async Task RequestScanForSelectedTargetAsync<T>(ObservableCollection<T> target) where T : DirectoryNode
        {
            CurrentScanningCancellationTokenSource = new CancellationTokenSource();
            PauseResetTokenSource = new PauseResetTokenSource();

            await ProceedScanForSelectedTargetAsync(target, CurrentScanningCancellationTokenSource, PauseResetTokenSource);
        }

        private async Task ProceedScanForSelectedTargetAsync<T>(ObservableCollection<T> target, CancellationTokenSource cts, PauseResetTokenSource prts) where T : DirectoryNode
        {
            long targetSizeSum;
            long startTime;
            TimeSpan elapsedTime;

            var progress = new Action<List<FileSystemNode>>(ProcessReceivedScannedNodes);
            ResetValuesAndCollections();

            ApplicationState.CurrentScanningState = AppState.ScanningStates.InProgress;
            startTime = TimeProvider.GetTimestamp();

            // Scans the first level of every root node.
            foreach (DirectoryNode directoryNode in target)
            {
                directoryNode.IsInProgress = true;
                ProceedScanForSelectedDirectoryLevel(directoryNode);
            }
            // Scanning.
            await DriveUtilsService.ScanProvidedNodesAsync<T>(target, progress, cts.Token, prts.Token);

            foreach (DirectoryNode directoryNode in target)
            {
                directoryNode.IsInProgress = false;
                directoryNode.IsExpanded = true;
            }

            // Calls OnPropertyChanged events after scanning for specific properties.
            UpdateInnerExpandedNodes(DriveNodes);

            targetSizeSum = target.Sum(dn => dn.Size);

            // Fills list with file categories by their extensions.
            foreach (FileExtensionItem item in FileExtentionItemService.GetOrderedExtensionCollection())
            {
                ApplicationState.FileExtensionItems.Add(item);
                item.UpdateParameters(targetSizeSum);
            }

            UpdateChart(ApplicationState.FileExtensionItems);
            // Build hierarchical TreemapNodes structure.
            BuildHierarchicalTreemapStructure(DriveNodes);

            if (CurrentScanningCancellationTokenSource != null)
            {
                CurrentScanningCancellationTokenSource.Dispose();
            }

            elapsedTime = TimeProvider.GetElapsedTime(startTime);

            if (ApplicationState.CurrentScanningState == AppState.ScanningStates.Canceled)
            {
                NotificationService.ShowCancelScanningNotification();
                return;
            }

            ApplicationState.CurrentScanningState = AppState.ScanningStates.Completed;

            long totalScannedDirectories = target.Sum(d => d.DirectoriesCount) + target.Count;
            long totalScannedFiles = target.Sum(d => d.FileCount);

            ScanReport scanReport = new ScanReport()
            {
                ScanDateTime = DateTime.Now,
                ElapsedTime = elapsedTime,
                NodesScanned = target.Count,
                TotalSize = targetSizeSum,
                TotalFilesCount = totalScannedFiles,
                TotalDirectoriesCount = totalScannedDirectories,
                RootNodes = new ObservableCollection<DirectoryNode>(target.Select((node) =>
                {
                    DirectoryNode directoryNode = new DirectoryNode(null)
                    {
                        Name = node.Name,
                        FullPath = node.FullPath,
                        Size = node.Size,
                        LastModified = node.LastModified,
                        UnicodeIcon = node.UnicodeIcon,
                        IconColor = node.IconColor,
                        FileCount = node.FileCount,
                        DirectoriesCount = node.DirectoriesCount,
                    };
                    return directoryNode;
                }))
            };
            string reportFilePath = ReportStorageService.SaveReport(scanReport);
            NotificationService.ShowSuccessScanningNotification(elapsedTime, target.Count, target.Sum(n => n.DirectoriesCount), target.Sum(n => n.FileCount), reportFilePath);
        }

        private void LoadAvailableDrives()
        {
            AllAvailableDrives.Clear();
            var drives = DriveUtilsService.GetAvailableDrives();
            foreach (DriveInfo drive in drives)
            {
                AllAvailableDrives.Add(drive);
            }
        }

        private void RemoveDataForRescannedDirectory(DirectoryNode directoryNode)
        {
            long fileCount = directoryNode.FileCount;
            long size = directoryNode.Size;
            var current = directoryNode;

            while (current != null)
            {
                current.Size -= size;
                current.FileCount -= fileCount;
                current = current.ParentNode as DirectoryNode;
            }
        }

        private void ProcessReceivedScannedNodes(List<FileSystemNode> data)
        {
            DispatcherQueueProvider.DispatcherQueue.TryEnqueue(() =>
            {
                ScanDataAggregator.AggregateNodes(data, fileNode =>
                {
                    FileExtentionItemService.UpdateOrCreateFileExtensionItem(fileNode.Extension, fileNode.Size, 1);
                });
            });
        }

        private void ResetValuesAndCollections()
        {
            FileExtentionItemService.ClearFileExtensionCollection();
            ApplicationState.FileExtensionSeriesCollection.Clear();
            ApplicationState.FileExtensionItems.Clear();
            ApplicationState.ScannedRootNodeNames.Clear();
        }

        private void ProceedScanForSelectedDirectoryLevel(DirectoryNode directoryNode)
        {
            TotalScanValues values = DriveUtilsService.ScanDirectoryLevel(directoryNode, directoryNode.FullPath);
            ApplicationState.ScannedRootNodeNames.Add(directoryNode.FullPath);

            var current = directoryNode;
            while (current != null)
            {
                current.Size += values.TotalSizeInBytes;
                current.FileCount += values.TotalFileCount;
                current = current.ParentNode as DirectoryNode;
            }
        }

        private void UpdateInnerExpandedNodes<T>(ObservableCollection<T> nodes) where T : DirectoryNode
        {
            foreach (DirectoryNode drive in nodes)
            {
                drive.UpdatePercentProperty();
                drive.UpdateFileCountProperty();
                drive.UpdateSizeProperty();

                if (drive.IsExpanded == true)
                {
                    RefreshExpandedNodesRecursive(drive.FileSystemNodes!);
                }
            }
        }

        private void RefreshExpandedNodesRecursive(ObservableCollection<FileSystemNode> nodes)
        {
            foreach (FileSystemNode node in nodes)
            {
                node.UpdatePercentProperty();
                node.UpdateSizeProperty();

                if (node is DirectoryNode directoryNode)
                {
                    directoryNode.UpdateFileCountProperty();

                    if (directoryNode.IsExpanded == true && directoryNode.FileSystemNodes!.Any())
                    {
                        RefreshExpandedNodesRecursive(directoryNode.FileSystemNodes!);
                    }
                }
            }
        }

        private void BuildHierarchicalTreemapStructure<T>(ObservableCollection<T> rootNodes) where T : DirectoryNode
        {
            TreemapNodes = new ObservableCollection<TreemapNode>(TreemapBuilderHelper.Build(rootNodes));
        }

        public void UpdateChart(IEnumerable<FileExtensionItem> fileExtensionItems)
        {
            ApplicationState.FileExtensionSeriesCollection = new ObservableCollection<ISeries>(ChartSeriesBuilderHelper.Build(fileExtensionItems));
        }
    }      
}