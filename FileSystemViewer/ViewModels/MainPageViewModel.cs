using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileSystemViewer.Models;
using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels.Tools;
using FileSystemViewer.Views.DialogPages;
using FileSystemViewer.Views.Windows;
using Humanizer;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ModernControls.Models;
using SkiaSharp;
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
        public MainPageViewModel(IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider, 
            IFileExtentionItemService fileExtentionItemService, IConfigurationService<AppSettings> configurationService, IVisualManagerService visualManagerService, AppState appState, 
            TimeProvider timeProvider) : base(driveUtilsService, dispatcherQueueProvider, fileExtentionItemService, configurationService, visualManagerService, appState)
        {
            DriveNodes = new ObservableCollection<DriveNode>();
            AllAvailableDrives = new ObservableCollection<DriveInfo>();
            SelectedTargetDrives = new ObservableCollection<DriveInfo>();
            TreemapNodes = new ObservableCollection<TreemapNode>();

            TimeProvider = timeProvider;

            SelectedScanningTargetIndex = 0;
            ApplicationState.CurrentScanningState = AppState.ScanningStates.None;

            ApplicationState.ScanningStatePropertyChanged += ApplicationState_ScanningStatePropertyChanged;
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

        TimeProvider TimeProvider { get; }
        private CancellationTokenSource? CurrentScanningCancellationTokenSource { get; set; }
        private PauseResetTokenSource? PauseResetTokenSource { get; set; } 

        /// <summary>
        /// Main nodes collection, contains all selected drives with their inner collections.
        /// </summary>
        public ObservableCollection<DriveNode> DriveNodes { get; set; }
        public ObservableCollection<DriveInfo> AllAvailableDrives { get; set; }
        public ObservableCollection<DriveInfo> SelectedTargetDrives { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<TreemapNode>? TreemapNodes { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RescanSelectedDirectoriesCommand))]
        public partial FileSystemNode? SelectedFileSystemNode { get; set; }

        // Selection mode: all drives (0) or selected (1).
        [ObservableProperty]
        public partial int SelectedScanningTargetIndex { get; set; }

        [ObservableProperty]
        public partial Visibility ProgressBarVisibility { get; set; }

        [RelayCommand(CanExecute = nameof(IsScanningOperationsAvailable))]
        public async Task OpenTargetSelectDialog(XamlRoot xamlRoot)
        {
            LoadAvailableDrives();
            SelectedTargetDrives.Clear();

            var dialogResult = await DialogManager.ShowContentDialogAsync(xamlRoot!, Localizer.GetLocalizedString("DialogTargetSelectionTitle"), Localizer.GetLocalizedString("DialogApplyText"),
                ContentDialogButton.Primary, new TargetSelectDialog(), Localizer.GetLocalizedString("DialogCancelText"), null);

            if (dialogResult == ContentDialogResult.Primary)
            {
                if (SelectedScanningTargetIndex == 1)
                {
                    if (!SelectedTargetDrives.Any()) { return; }

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
        public async Task RefreshScanning(XamlRoot xamlRoot)
        {
            if (!DriveNodes.Any())
                return;

            var dialogResult = await DialogManager.ShowContentDialogAsync(xamlRoot!, Localizer.GetLocalizedString("DialogRefreshScanningTitle"), Localizer.GetLocalizedString("DialogConfirmText"),
               ContentDialogButton.Primary, $"{Localizer.GetLocalizedString("DialogRefreshScanningText")} {DriveNodes.Count}?", Localizer.GetLocalizedString("DialogCancelText"), null);

            if(dialogResult == ContentDialogResult.Primary)
            {
                foreach (DirectoryNode driveNode in DriveNodes)
                {
                    driveNode.FileSystemNodes!.Clear();
                    driveNode.FileCount = 0;
                    driveNode.Size = 0;

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
        public async Task RescanSelectedDirectories(XamlRoot xamlRoot)
        {
            if (SelectedFileSystemNode != null && SelectedFileSystemNode is DirectoryNode selectedDirectoryNode)
            {
                var dialogResult = await DialogManager.ShowContentDialogAsync(xamlRoot!, Localizer.GetLocalizedString("DialogRefreshSelectedTitle"), Localizer.GetLocalizedString("DialogConfirmText"),
                   ContentDialogButton.Primary, $"{Localizer.GetLocalizedString("DialogRefreshSelectedText")}", Localizer.GetLocalizedString("DialogCancelText"), null);

                if (dialogResult == ContentDialogResult.Primary)
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
        public async Task CancelScanning(XamlRoot xamlRoot)
        {
            var dialogResult = await DialogManager.ShowContentDialogAsync(xamlRoot!, Localizer.GetLocalizedString("DialogCancelScanningTitle"), Localizer.GetLocalizedString("DialogConfirmText"),
                ContentDialogButton.Primary, Localizer.GetLocalizedString("DialogCancelScanningText"), Localizer.GetLocalizedString("DialogCancelText"), null);

            if (dialogResult == ContentDialogResult.Primary)
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
            string windowKey = nameof(TreeViewWindow);

            if (!ApplicationState.ActiveSubWindows.ContainsKey(windowKey))
            {
                TreeViewWindow treeViewWindow = new TreeViewWindow();
                VisualManagerService.SetWindowTheme(treeViewWindow, ConfigurationService.Settings!.AppTheme);
                treeViewWindow.Closed += (s, e) => ApplicationState.ActiveSubWindows.Remove(windowKey);
                ApplicationState.ActiveSubWindows.Add(windowKey, treeViewWindow);
                treeViewWindow.Activate();
            }
        }

        [RelayCommand]
        public void OpenTreeMapNewWindow()
        {
            string windowKey = nameof(TreemapWindow);

            if (!ApplicationState.ActiveSubWindows.ContainsKey(windowKey))
            {
                TreemapWindow treemapWindow = new TreemapWindow();
                VisualManagerService.SetWindowTheme(treemapWindow, ConfigurationService.Settings!.AppTheme);
                treemapWindow.Closed += (s, e) => ApplicationState.ActiveSubWindows.Remove(windowKey);
                ApplicationState.ActiveSubWindows.Add(windowKey, treemapWindow);
                treemapWindow.Activate();
            }
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
                string cancelImagePath = Path.Combine(AppContext.BaseDirectory, "Assets", "cancel.png");
                NotificationManager.BuildAndShowToastNotification(
                    Localizer.GetLocalizedString("NotificationCanceledTitle"),
                    Localizer.GetLocalizedString("NotificationCanceledText"),
                    cancelImagePath
                );
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
            string reportFilePath = ScanningReportHelper.GenerateReportAsJson(scanReport);

            string successImagePath = Path.Combine(AppContext.BaseDirectory, "Assets", "success.png");
            NotificationManager.BuildAndShowToastNotification(
                Localizer.GetLocalizedString("NotificationSuccessTitle"),
                $"{Localizer.GetLocalizedString("NotificationSuccessText1")} {totalScannedDirectories}; {Localizer.GetLocalizedString("NotificationSuccessText2")} " +
                $"{totalScannedFiles};\n{Localizer.GetLocalizedString("NotificationSuccessText3")} {elapsedTime.Humanize()}.",
                successImagePath, reportFilePath);
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
                Dictionary<DirectoryNode, TotalScanValues> totalScanValues = new Dictionary<DirectoryNode, TotalScanValues>();

                foreach (FileSystemNode node in data)
                {
                    DirectoryNode parentNode = (node.ParentNode as DirectoryNode)!;
                    long fileSize = 0;
                    long directoriesCount = 0;
                    long fileCount = 0;

                    parentNode.FileSystemNodes!.Add(node);

                    if (node is DirectoryNode)
                    {
                        directoriesCount++;
                    }
                    else if (node is FileNode fileNode)
                    {
                        fileSize = fileNode.Size;
                        fileCount++;
                        FileExtentionItemService.UpdateOrCreateFileExtensionItem(fileNode.Extension, fileNode.Size, 1);
                    }

                    if (totalScanValues.TryGetValue(parentNode, out var values))
                    {
                        values.TotalSizeInBytes += fileSize;
                        values.TotalDirectoryCount += directoriesCount;
                        values.TotalFileCount += fileCount;
                    }
                    else
                    {
                        values = new TotalScanValues();
                        values.TotalSizeInBytes = fileSize;
                        values.TotalDirectoryCount = directoriesCount;
                        values.TotalFileCount = fileCount;
                        totalScanValues.Add(parentNode, values);
                    }
                }

                foreach (KeyValuePair<DirectoryNode, TotalScanValues> pair in totalScanValues)
                {
                    var current = pair.Key;

                    while (current != null)
                    {
                        current.Size += pair.Value.TotalSizeInBytes;
                        current.FileCount += pair.Value.TotalFileCount;
                        current.DirectoriesCount += pair.Value.TotalDirectoryCount;
                        current = current.ParentNode as DirectoryNode;
                    }
                }
            });   
        }

        private void ResetValuesAndCollections()
        {
            FileExtentionItemService.ClearFileExtensionCollection();
            ApplicationState.FileExtensionSeriesCollection.Clear();
            ApplicationState.FileExtensionItems.Clear();
            ApplicationState.ScannedRootNodeNames.Clear();

            ApplicationState.TotalFilesScanned = 0;
            ApplicationState.TotalDirectoriesScanned = 0;
        }

        private void ProceedScanForSelectedDirectoryLevel(DirectoryNode directoryNode)
        {
            TotalScanValues values = DriveUtilsService.ScanDirectoryLevel(directoryNode, directoryNode.FullPath);
            ApplicationState.ScannedRootNodeNames.Add(directoryNode.FullPath);

            ApplicationState.TotalDirectoriesScanned += values.TotalDirectoryCount;
            ApplicationState.TotalFilesScanned += values.TotalFileCount;

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
            var newTreemanCollection = new ObservableCollection<TreemapNode>();

            foreach (var rootNode in rootNodes)
            {
                var rootTreemapNode = CreateTreemapNodeFromDirectoryNode(rootNode);

                if (rootTreemapNode == null)
                    return;

                newTreemanCollection.Add(rootTreemapNode);
            }

            TreemapNodes = newTreemanCollection;
        }

        private TreemapNode? CreateTreemapNodeFromDirectoryNode(DirectoryNode directoryNode)
        {
            const int maxSubdirectories = 200;
            const double minPercent = 0.01;

            var treemapNode = new TreemapNode
            {
                LabeledName = directoryNode.Name,
                IsContainer = true,
                Size = directoryNode.Size,
                BackgroundColor = ColorManager.DirectoryTreemapNodeColor,
                Percent = 0,
                Children = new ObservableCollection<TreemapNode>(),
                FullPath = directoryNode.FullPath
            };

            var subDirectories = directoryNode.FileSystemNodes!
                .OfType<DirectoryNode>();
                
            var orderedDirectories = subDirectories
                .Where(d => d.PercentProperty >= minPercent)
                .OrderByDescending(d => d.PercentProperty)
                .Take(maxSubdirectories)
                .ToList();

            var extensionGroups = directoryNode.FileSystemNodes!
                .OfType<FileNode>()
                .GroupBy(f => f.Extension);
                
            var sortedFilesExtensionGroups = extensionGroups
                .Where(g => g.Sum(i => i.PercentProperty) >= minPercent)
                .ToList();

            // File extension groups
            foreach (var extensionGroup in sortedFilesExtensionGroups)
            {
                string extension = extensionGroup.Key;
                long totalSizeForExtension = extensionGroup.Sum(f => f.Size);
                double totalPercent = extensionGroup.Sum(f => f.PercentProperty);

                var fileExtensionNode = new TreemapNode
                {
                    LabeledName = string.IsNullOrEmpty(extension) ? "No extension" : extension,
                    IsContainer = false,
                    Size = totalSizeForExtension,
                    BackgroundColor = ColorManager.GetColorByExtension(extension),
                    Percent = totalPercent,
                    Parent = treemapNode,
                    Children = null
                };

                treemapNode.Children.Add(fileExtensionNode);
            }

            // Directories
            foreach (var subDirectory in orderedDirectories)
            {
                var childTreemapNode = CreateTreemapNodeFromDirectoryNode(subDirectory);

                if (childTreemapNode == null)
                    continue;

                childTreemapNode.Parent = treemapNode;
                treemapNode.Children.Add(childTreemapNode);
            }

            // Calculate percentages for this node's children
            if (treemapNode.Children.Any())
            {
                long totalChildSize = treemapNode.Children.Sum(c => c.Size);
                if (totalChildSize > 0)
                {
                    foreach (var child in treemapNode.Children)
                    {
                        child.Percent = (double)child.Size / totalChildSize * 100;
                    }
                }
            }

            return treemapNode;
        }

        public void UpdateChart(IEnumerable<FileExtensionItem> fileItems)
        {
            ApplicationState.FileExtensionSeriesCollection.Clear();

            List<FileExtensionItem> otherItems = new List<FileExtensionItem>();

            var validItems = fileItems
                .Where((item) => 
                { 
                    if (item.Percent <= 1)
                    {
                        otherItems.Add(item);
                        return false;
                    }

                    return true; 

                }).ToList();

            var seriesList = validItems.Select(TransformIntoSeries);
            var otherSeries = ArrangeOtherSeries(otherItems);

            foreach (var series in seriesList)
            {
                ApplicationState.FileExtensionSeriesCollection.Add(series);
            }
            ApplicationState.FileExtensionSeriesCollection.Add(otherSeries);
        }

        private ISeries TransformIntoSeries(FileExtensionItem item)
        {
            var pieSeries = new PieSeries<long>
            {
                Values = new long[] { item.Size },
                Name = item.Extension,
                ToolTipLabelFormatter = point => $"{item.Percent:F2}%",
                InnerRadius = 0,
                HoverPushout = 5,
                Pushout = 2
            };

            var winColor = item.Color;
            pieSeries.Fill = new SolidColorPaint(new SKColor(winColor.R, winColor.G, winColor.B, winColor.A));

            return pieSeries;
        }

        private ISeries ArrangeOtherSeries(List<FileExtensionItem> others)
        {
            PieSeries<long> pieSeries = new PieSeries<long>()
            {
                Values = new long[] { others.Sum(i => i.Size) },
                Name = "Other",
                ToolTipLabelFormatter = point => $"{others.Sum(i => i.Percent):F2}%",
                InnerRadius = 0,
                HoverPushout = 5,
                Pushout = 2
            };

            var otherColor = ColorManager.OtherColor;
            pieSeries.Fill = new SolidColorPaint(new SKColor(otherColor.R, otherColor.G, otherColor.B, otherColor.A));

            return pieSeries;
        }
    }
}
