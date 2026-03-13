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
        private CancellationTokenSource? CurrentScanningCancellationTokenSource { get; set; } // Для отмены
        private PauseResetTokenSource? PauseResetTokenSource { get; set; } // Для паузы/возобновления

        /// <summary>
        /// Главная коллекция, содержит перечень дисков со всеми сопутствующими вложениями
        /// </summary>
        public ObservableCollection<DriveNode> DriveNodes { get; set; }
        /// <summary>
        /// Все доступные диски на устройстве.
        /// </summary>
        public ObservableCollection<DriveInfo> AllAvailableDrives { get; set; }
        /// <summary>
        /// Выбранные диски среди доступных.
        /// </summary>
        public ObservableCollection<DriveInfo> SelectedTargetDrives { get; set; }
        /// <summary>
        /// Выбранные (выделенные) узлы директорий 
        /// </summary>
        public ObservableCollection<DirectoryNode> SelectedDirectoryNodes { get; set; }

        // Режим выбора: Все диски или выбранные
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

        // Открывает диалог выбора целей для сканирования (дисков)
        private ICommand? _openTargetSelectDialogCommand;
        public ICommand OpenTargetSelectDialogCommand => _openTargetSelectDialogCommand ??= new RelayCommand<XamlRoot>(async (xamlRoot) =>
        {
            LoadAvailableDrives();

            SelectedTargetDrives.Clear();

            var dialogResult = await DialogManager.ShowContentDialog(xamlRoot!, "Target selection...", "Apply",
                ContentDialogButton.Primary, new TargetSelectDialog(), "Cancel", null);

            if (dialogResult == ContentDialogResult.Primary)
            {
                if(SelectedScanningTargetIndex == 1)
                {
                    if (!SelectedTargetDrives.Any())
                    {
                        // TODO: Показать уведомление об ошибке.
                        return;
                    }

                    DriveNodes.Clear();

                    if (CurrentScanningCancellationTokenSource != null)
                        CurrentScanningCancellationTokenSource.Dispose();

                    CurrentScanningCancellationTokenSource = new CancellationTokenSource();
                    PauseResetTokenSource = new PauseResetTokenSource();

                    foreach (DriveInfo drive in SelectedTargetDrives)
                    {
                        DriveNodes.Add(new DriveNode(drive.VolumeLabel, drive.TotalSize, drive.TotalFreeSpace, drive.Name, drive.RootDirectory.FullName, 0, drive.RootDirectory.LastWriteTime));
                    }
                    await ScanSelectedTargetAsync(DriveNodes, CurrentScanningCancellationTokenSource, PauseResetTokenSource);
                }
                else
                {
                    DriveNodes.Clear();

                    CurrentScanningCancellationTokenSource = new CancellationTokenSource();
                    PauseResetTokenSource = new PauseResetTokenSource();

                    foreach (DriveInfo drive in AllAvailableDrives)
                    {
                        DriveNodes.Add(new DriveNode(drive.VolumeLabel, drive.TotalSize, drive.TotalFreeSpace, drive.Name, drive.RootDirectory.FullName, 0, drive.RootDirectory.LastWriteTime));
                    }
                    await ScanSelectedTargetAsync(DriveNodes, CurrentScanningCancellationTokenSource, PauseResetTokenSource);
                }
            }
            
        }, (xamltoor) => CurrentScanningState == ScanningStates.None || CurrentScanningState == ScanningStates.Completed || CurrentScanningState == ScanningStates.Canceled);


        // Обновляет список доступных дисков, в меню выбора целей для сканирования
        private ICommand? _refreshAvailableDrivesCollectionCommand;
        public ICommand RefreshAvailableDrivesCollectionCommand => _refreshAvailableDrivesCollectionCommand ??= new RelayCommand(() =>
        {
            LoadAvailableDrives();
        });


        // Запускает сканирование целевых дисков заново
        private ICommand? _refreshScanningCommand;
        public ICommand RefreshScanningCommand => _refreshScanningCommand ??= new RelayCommand<XamlRoot>(async (xamlRoot) =>
        {
            if (!DriveNodes.Any())
                return;

            var dialogResult = await DialogManager.ShowContentDialog(xamlRoot!, "Rescan target confirmation", "Confirm",
               ContentDialogButton.Primary, $"Are you sure you want to rescan the following count of drives: " +
               $"{DriveNodes.Count}?", "Cancel", null);

            if(dialogResult == ContentDialogResult.Primary)
            {
                foreach(DirectoryNode drive in DriveNodes)
                {
                    drive.FileSystemNodes.Clear();
                    drive.FileCount = 0;
                    drive.Size = 0;
                }

                if (CurrentScanningCancellationTokenSource != null)
                    CurrentScanningCancellationTokenSource.Dispose();

                CurrentScanningCancellationTokenSource = new CancellationTokenSource();
                PauseResetTokenSource = new PauseResetTokenSource();

                await ScanSelectedTargetAsync(DriveNodes, CurrentScanningCancellationTokenSource, PauseResetTokenSource);
            }
        }, (xamlRoot) => CurrentScanningState == ScanningStates.None || CurrentScanningState == ScanningStates.Completed || CurrentScanningState == ScanningStates.Canceled);


        // Запускает повторное сканирование для выбранной директории
        private ICommand? _rescanSelectedDirectoriesCommand;
        public ICommand RescanSelectedDirectoriesCommand => _rescanSelectedDirectoriesCommand ??= new RelayCommand<XamlRoot>(async (xamlRoot) =>
        {
            if (SelectedDirectoryNodes.Any() && DirectoriesSelectionMode == TreeViewSelectionMode.Multiple)
            {
                var dialogResult = await DialogManager.ShowContentDialog(xamlRoot!, "Rescan targets confirmation", "Confirm",
                   ContentDialogButton.Primary, $"Are you sure you want to rescan the selected directories?", "Cancel", null);

                if (dialogResult == ContentDialogResult.Primary)
                {
                    if (CurrentScanningCancellationTokenSource != null)
                        CurrentScanningCancellationTokenSource.Dispose();

                    CurrentScanningCancellationTokenSource = new CancellationTokenSource();
                    PauseResetTokenSource = new PauseResetTokenSource();

                    foreach (var directoryNode in SelectedDirectoryNodes)
                    {
                        directoryNode.FileSystemNodes.Clear();
                        directoryNode.FileCount = 0;
                        directoryNode.Size = 0;
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

        // Отменяет процесс сканирование
        private ICommand? _cancelScanningCommand;
        public ICommand CancelScanningCommand => _cancelScanningCommand ??= new RelayCommand<XamlRoot>(async (xamlRoot) =>
        {
            var dialogResult = await DialogManager.ShowContentDialog(xamlRoot!, "Cancel scanning confirmation", "Yes",
                ContentDialogButton.Primary, $"Are you sure you want to cancel the scanning process?", "No", null);

            if (dialogResult == ContentDialogResult.Primary)
            {
                CurrentScanningCancellationTokenSource!.Cancel();
                CurrentScanningState = ScanningStates.Canceled;
            }

        }, (xamlRoot) => CurrentScanningState == ScanningStates.InProgress || CurrentScanningState == ScanningStates.Paused);

        // Возобновляет процесс сканирования после паузы
        private ICommand? _resumeScanningCommand;
        public ICommand ResumeScanningCommand => _resumeScanningCommand ??= new RelayCommand(async () =>
        {
            PauseResetTokenSource!.Reset();
            CurrentScanningState = ScanningStates.InProgress;

        }, () => CurrentScanningState == ScanningStates.Paused);

        // Ставит процесс сканирования на паузу
        private ICommand? _pauseScanningCommand;
        public ICommand PauseScanningCommand => _pauseScanningCommand ??= new RelayCommand(async () =>
        {
            PauseResetTokenSource!.Pause();
            CurrentScanningState = ScanningStates.Paused;

        }, () => CurrentScanningState == ScanningStates.InProgress);

        // Устанавливает выбранную директорию
        /*
         Поскольку привязка коллекции к TreeView отсутствует, выбранным элементом при прямой привязке стал бы TreeViewItem,
         поэтому в code behind главной страницы вручную обрабатывается событие изменения и передает информацию сюда.
         */
        private ICommand? _fileSystemNodeSelectionChanged;
        public ICommand FileSystemNodeSelectionChanged => _fileSystemNodeSelectionChanged ??= new RelayCommand<IList<object>>(async (nodes) =>
        {
            if(nodes == null)
            {
                SelectedDirectoryNodes.Clear();
                return;
            }

            foreach (var node in nodes)
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
        /// <summary>
        /// Сканирует выбранную цель.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="cts"></param>
        /// <param name="prts"></param>
        /// <returns></returns>
        private async Task ScanSelectedTargetAsync<T>(ObservableCollection<T> target, CancellationTokenSource cts, PauseResetTokenSource prts) where T : DirectoryNode
        {
            var progress = new Progress<List<FileSystemNode>>(data =>
            {
                //Хранит значения размера и количества файлов для каждого из родильских узлов в этом наборе data(до 100 узлов)
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

                foreach (var pair in totalScanValues)
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

            CurrentScanningState = ScanningStates.InProgress;

            await DriveUtilsService.ScanProvidedNodesAsync<T>(target, progress, cts.Token, prts.Token);

            foreach (var drive in DriveNodes)
            {
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

        /// <summary>
        /// Загружает перечень доступных системе дисков, и добавляет их в коллекцию
        /// </summary>
        private void LoadAvailableDrives()
        {
            AllAvailableDrives.Clear();
            var drives = DriveUtilsService.GetAvailableDrives();
            foreach (var drive in drives)
            {
                AllAvailableDrives.Add(drive);
            }
        }

        private void RefreshExpandedNodesRecursive(ObservableCollection<FileSystemNode> nodes)
        {
            foreach (var node in nodes)
            {
                node.UpdatePercentForUI();

                if (node is DirectoryNode directoryNode && directoryNode.IsExpanded && directoryNode.FileSystemNodes.Any())
                {
                    RefreshExpandedNodesRecursive(directoryNode.FileSystemNodes);
                }
            }
        }
        #endregion
    }
}
