using CommunityToolkit.Mvvm.Input;
using FileSystem_Viewer.Models.DataModels;
using FileSystemViewer.Models;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels.Tools;
using FileSystemViewer.Views.DialogPages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FileSystemViewer.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel(IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider) : base(driveUtilsService, dispatcherQueueProvider)
        {
            DriveNodes = new ObservableCollection<DriveNode>();
            AllAvailableDrives = new ObservableCollection<DriveInfo>();
            SelectedTargetDrives = new ObservableCollection<DriveInfo>();
            SelectedDirectoryNodes = new ObservableCollection<DirectoryNode>();

            SelectedScanningTargetIndex = 0;
            CurrentScanningState = ScanningStates.None;
            DirectoriesSelectionMode = TreeViewSelectionMode.None;
        }

        #region Properties

        public enum ScanningStates
        {
            None,
            Completed,
            Paused,
            Canceled,
            InProgress
        }

        private CancellationTokenSource? CurrentScanningCancellationTokenSource { get; set; } 
        
        private PauseResetTokenSource? PauseResetTokenSource { get; set; } 

        /// <summary>
        /// Main nodes collection, contains all selected drives with their inner collections.
        /// </summary>
        public ObservableCollection<DriveNode> DriveNodes { get; set; }
        public ObservableCollection<DriveInfo> AllAvailableDrives { get; set; }
        public ObservableCollection<DriveInfo> SelectedTargetDrives { get; set; }
        public ObservableCollection<DirectoryNode> SelectedDirectoryNodes { get; set; }

        // Selection mode: All drives (0) or selected (1).
        private int _selectedScanningTargetIndex;
        public int SelectedScanningTargetIndex
        {
            get { return _selectedScanningTargetIndex; }
            set { SetProperty(ref _selectedScanningTargetIndex, value); }
        }

        private ScanningStates _currentScanningState;
        public ScanningStates CurrentScanningState
        {
            get { return _currentScanningState; }
            set 
            { 
                if (SetProperty(ref _currentScanningState, value))
                {
                    (OpenTargetSelectDialogCommand as RelayCommand<XamlRoot>)!.NotifyCanExecuteChanged();
                    (RefreshScanningCommand as RelayCommand<XamlRoot>)!.NotifyCanExecuteChanged();
                    (RescanSelectedDirectoriesCommand as RelayCommand<XamlRoot>)!.NotifyCanExecuteChanged();
                    (CancelScanningCommand as RelayCommand<XamlRoot>)!.NotifyCanExecuteChanged();
                    (ResumeScanningCommand as RelayCommand)!.NotifyCanExecuteChanged();
                    (PauseScanningCommand as RelayCommand)!.NotifyCanExecuteChanged();

                    (ResumeScanningCommand as RelayCommand)!.NotifyCanExecuteChanged();
                    (PauseScanningCommand as RelayCommand)!.NotifyCanExecuteChanged();
                } 
            }
        }

        private TreeViewSelectionMode _directoriesSelectionMode;
        public TreeViewSelectionMode DirectoriesSelectionMode
        {
            get { return _directoriesSelectionMode; }
            set 
            { 
                if (SetProperty(ref _directoriesSelectionMode, value))
                {
                    (RescanSelectedDirectoriesCommand as RelayCommand<XamlRoot>)!.NotifyCanExecuteChanged();
                } 
            }
        }

        private Visibility _progressBarVisibility;
        public Visibility ProgressBarVisibility
        {
            get { return _progressBarVisibility; }
            set { SetProperty(ref _progressBarVisibility, value); }
        }

        #endregion

        #region Commands

        private ICommand? _openTargetSelectDialogCommand;
        public ICommand OpenTargetSelectDialogCommand => _openTargetSelectDialogCommand ??= new RelayCommand<XamlRoot>(async (xamlRoot) =>
        {
            LoadAvailableDrives();

            SelectedTargetDrives.Clear();

            var dialogResult = await DialogManager.ShowContentDialogAsync(xamlRoot!, "Target selection...", "Apply",
                ContentDialogButton.Primary, new TargetSelectDialog(), "Cancel", null);

            if (dialogResult == ContentDialogResult.Primary)
            {
                if (SelectedScanningTargetIndex == 1)
                {
                    if (!SelectedTargetDrives.Any()) { return; }

                    DriveNodes.Clear();

                    if (CurrentScanningCancellationTokenSource != null)
                        CurrentScanningCancellationTokenSource.Dispose();

                    CurrentScanningCancellationTokenSource = new CancellationTokenSource();
                    PauseResetTokenSource = new PauseResetTokenSource();

                    foreach (DriveInfo driveInfo in SelectedTargetDrives)
                    {
                        DriveNodes.Add(new DriveNode(driveInfo.VolumeLabel, driveInfo.TotalSize, driveInfo.TotalFreeSpace, driveInfo.Name, driveInfo.RootDirectory.FullName, 0, driveInfo.RootDirectory.LastWriteTime));
                    }
                    await ScanSelectedTargetAsync(DriveNodes, CurrentScanningCancellationTokenSource, PauseResetTokenSource);
                }
                else
                {
                    DriveNodes.Clear();

                    CurrentScanningCancellationTokenSource = new CancellationTokenSource();
                    PauseResetTokenSource = new PauseResetTokenSource();

                    foreach (DriveInfo driveInfo in AllAvailableDrives)
                    {
                        DriveNodes.Add(new DriveNode(driveInfo.VolumeLabel, driveInfo.TotalSize, driveInfo.TotalFreeSpace, driveInfo.Name, driveInfo.RootDirectory.FullName, 0, driveInfo.RootDirectory.LastWriteTime));
                    }
                    await ScanSelectedTargetAsync(DriveNodes, CurrentScanningCancellationTokenSource, PauseResetTokenSource);
                }
            }
            
        }, (xamltoor) => CurrentScanningState == ScanningStates.None || CurrentScanningState == ScanningStates.Completed || CurrentScanningState == ScanningStates.Canceled);


        // Updates drives list in selection target menu
        private ICommand? _refreshAvailableDrivesCollectionCommand;
        public ICommand RefreshAvailableDrivesCollectionCommand => _refreshAvailableDrivesCollectionCommand ??= new RelayCommand(() =>
        {
            LoadAvailableDrives();
        });

        
        // Starts scanning target again
        private ICommand? _refreshScanningCommand;
        public ICommand RefreshScanningCommand => _refreshScanningCommand ??= new RelayCommand<XamlRoot>(async (xamlRoot) =>
        {
            if (!DriveNodes.Any())
                return;

            var dialogResult = await DialogManager.ShowContentDialogAsync(xamlRoot!, "Rescan target confirmation", "Confirm",
               ContentDialogButton.Primary, $"Are you sure you want to rescan the following count of drives: " +
               $"{DriveNodes.Count}?", "Cancel", null);

            if(dialogResult == ContentDialogResult.Primary)
            {
                foreach (DirectoryNode driveNode in DriveNodes)
                {
                    driveNode.FileSystemNodes.Clear();
                    driveNode.FileCount = 0;
                    driveNode.Size = 0;

                    driveNode.UpdateSizeProperty();
                    driveNode.UpdateFileCountProperty();
                }

                if (CurrentScanningCancellationTokenSource != null)
                    CurrentScanningCancellationTokenSource.Dispose();

                CurrentScanningCancellationTokenSource = new CancellationTokenSource();
                PauseResetTokenSource = new PauseResetTokenSource();

                await ScanSelectedTargetAsync(DriveNodes, CurrentScanningCancellationTokenSource, PauseResetTokenSource);
            }
        }, (xamlRoot) => CurrentScanningState == ScanningStates.None || CurrentScanningState == ScanningStates.Completed || CurrentScanningState == ScanningStates.Canceled);


        // Starts scanning target again for selected directory nodes
        private ICommand? _rescanSelectedDirectoriesCommand;
        public ICommand RescanSelectedDirectoriesCommand => _rescanSelectedDirectoriesCommand ??= new RelayCommand<XamlRoot>(async (xamlRoot) =>
        {
            if (SelectedDirectoryNodes.Any() && DirectoriesSelectionMode == TreeViewSelectionMode.Multiple)
            {
                var dialogResult = await DialogManager.ShowContentDialogAsync(xamlRoot!, "Rescan targets confirmation", "Confirm",
                   ContentDialogButton.Primary, $"Are you sure you want to rescan the selected directories?", "Cancel", null);

                if (dialogResult == ContentDialogResult.Primary)
                {
                    if (CurrentScanningCancellationTokenSource != null)
                        CurrentScanningCancellationTokenSource.Dispose();

                    CurrentScanningCancellationTokenSource = new CancellationTokenSource();
                    PauseResetTokenSource = new PauseResetTokenSource();

                    foreach (DirectoryNode directoryNode in SelectedDirectoryNodes)
                    {
                        directoryNode.FileSystemNodes.Clear();
                        directoryNode.FileCount = 0;
                        directoryNode.Size = 0;

                        directoryNode.UpdateSizeProperty();
                        directoryNode.UpdateFileCountProperty();
                        directoryNode.UpdatePercentProperty();
                    }

                    await ScanSelectedTargetAsync(SelectedDirectoryNodes, CurrentScanningCancellationTokenSource, PauseResetTokenSource);
                }
            }
        }, (xamlRoot) => (CurrentScanningState == ScanningStates.Completed || CurrentScanningState == ScanningStates.Canceled) && (DirectoriesSelectionMode == TreeViewSelectionMode.Multiple));

        private ICommand? _switchSelectionModeCommand;
        public ICommand SwitchSelectionModeCommand => _switchSelectionModeCommand ??= new RelayCommand(() =>
        {
            switch (DirectoriesSelectionMode)
            {
                case TreeViewSelectionMode.None:
                    DirectoriesSelectionMode = TreeViewSelectionMode.Multiple;
                    break;
                case TreeViewSelectionMode.Multiple:
                    DirectoriesSelectionMode = TreeViewSelectionMode.None;
                    break;
            }
        });


        #region Scanning managing commands

        private ICommand? _cancelScanningCommand;
        public ICommand CancelScanningCommand => _cancelScanningCommand ??= new RelayCommand<XamlRoot>(async (xamlRoot) =>
        {
            var dialogResult = await DialogManager.ShowContentDialogAsync(xamlRoot!, "Cancel scanning confirmation", "Yes",
                ContentDialogButton.Primary, $"Are you sure you want to cancel the scanning process?", "No", null);

            if (dialogResult == ContentDialogResult.Primary)
            {
                CurrentScanningCancellationTokenSource!.Cancel();
                CurrentScanningState = ScanningStates.Canceled;
            }

        }, (xamlRoot) => CurrentScanningState == ScanningStates.InProgress || CurrentScanningState == ScanningStates.Paused);

        private ICommand? _resumeScanningCommand;
        public ICommand ResumeScanningCommand => _resumeScanningCommand ??= new RelayCommand(async () =>
        {
            PauseResetTokenSource!.Reset();
            CurrentScanningState = ScanningStates.InProgress;

        }, () => CurrentScanningState == ScanningStates.Paused);

        private ICommand? _pauseScanningCommand;
        public ICommand PauseScanningCommand => _pauseScanningCommand ??= new RelayCommand(async () =>
        {
            PauseResetTokenSource!.Pause();
            CurrentScanningState = ScanningStates.Paused;

        }, () => CurrentScanningState == ScanningStates.InProgress);

        private ICommand? _fileSystemNodeSelectionChanged;
        public ICommand FileSystemNodeSelectionChanged => _fileSystemNodeSelectionChanged ??= new RelayCommand<IList<object>>(async (nodes) =>
        {
            if(nodes == null)
            {
                SelectedDirectoryNodes.Clear();
                return;
            }

            foreach (object node in nodes)
            {
                if (node is TreeViewNode treeViewNode && treeViewNode.Content is DirectoryNode directoryNode)
                {
                    SelectedDirectoryNodes.Add(directoryNode);
                }
            }
        });
        #endregion
        #endregion

        #region Methods
        private async Task ScanSelectedTargetAsync<T>(ObservableCollection<T> target, CancellationTokenSource cts, PauseResetTokenSource prts) where T : DirectoryNode
        {
            var progress = new Progress<List<FileSystemNode>>(data =>
            {
                Dictionary<DirectoryNode, TotalScanValues> totalScanValues = new Dictionary<DirectoryNode, TotalScanValues>();

                foreach (FileSystemNode node in data)
                {
                    DirectoryNode parentNode = (node.ParentNode as DirectoryNode)!;

                    parentNode.FileSystemNodes.Add(node);

                    if (node is FileNode fileNode)
                    {
                        var values = new TotalScanValues();

                        if (totalScanValues.TryGetValue(parentNode, out values))
                        {
                            values.TotalSizeInBytes += fileNode.Size;
                            values.TotalFileCount++;
                        }
                        else
                        {
                            values = new TotalScanValues();
                            values.TotalSizeInBytes = fileNode.Size;
                            values.TotalFileCount++;
                            totalScanValues.Add(parentNode, values);
                        }
                    }
                }

                foreach (KeyValuePair<DirectoryNode, TotalScanValues> pair in totalScanValues)
                {
                    var current = pair.Key;

                    while (current != null)
                    {
                        current.Size += pair.Value.TotalSizeInBytes;
                        current.FileCount += pair.Value.TotalFileCount;

                        if (target.Contains(current))
                            break;

                        current = current.ParentNode as DirectoryNode;
                    }
                }
            });

            // Created to change scanning status for particular drive/directory.
            var completeProgress = new Progress<DirectoryNode>(directoryNode =>
            {
                directoryNode.IsInProgress = false;
                directoryNode.UpdatePercentProperty();
                directoryNode.UpdateFileCountProperty();
                directoryNode.UpdateSizeProperty();
            });

            CurrentScanningState = ScanningStates.InProgress;

            foreach (DirectoryNode directoryNode in target)
            {
                directoryNode.IsInProgress = true;
                DriveUtilsService.ScanDirectoryLevel(directoryNode, directoryNode.FullPath);
            }

            await DriveUtilsService.ScanProvidedNodesAsync<T>(target, progress, completeProgress, cts.Token, prts.Token);

            foreach (DirectoryNode directoryNode in target)
            {
                directoryNode.IsInProgress = false;
            }

            foreach (DriveNode drive in DriveNodes)
            {
                drive.UpdateFileCountProperty();
                drive.UpdateSizeProperty();

                if (drive.IsExpanded)
                {
                    RefreshExpandedNodesRecursive(drive.FileSystemNodes);
                }
            }

            if (CurrentScanningCancellationTokenSource != null)
                CurrentScanningCancellationTokenSource.Dispose();

            if (CurrentScanningState == ScanningStates.Canceled)
                return;

            CurrentScanningState = ScanningStates.Completed;
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

        private void RefreshExpandedNodesRecursive(ObservableCollection<FileSystemNode> nodes)
        {
            foreach (FileSystemNode node in nodes)
            {
                node.UpdatePercentProperty();
                node.UpdateSizeProperty();

                if (node is DirectoryNode directoryNode)
                {
                    directoryNode.UpdateFileCountProperty();

                    if (directoryNode.IsExpanded && directoryNode.FileSystemNodes.Any())
                    {
                        RefreshExpandedNodesRecursive(directoryNode.FileSystemNodes);
                    }
                }
            }
        }
        #endregion
    }
}
