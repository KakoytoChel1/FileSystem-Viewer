using CommunityToolkit.Mvvm.Input;
using FileSystem_Viewer.Models;
using FileSystem_Viewer.Services.IServices;
using FileSystem_Viewer.ViewModels.Tools;
using FileSystem_Viewer.Views.DialogPages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FileSystem_Viewer.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel(IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider) : base(driveUtilsService, dispatcherQueueProvider)
        {
            DriveNodes = new ObservableCollection<DirectoryNode>();
            AllAvailableDrives = new ObservableCollection<DriveInfo>();
            SelectedTargetDrives = new ObservableCollection<DriveInfo>();

            PauseResetTokenSource = new PauseResetTokenSource();

            DriveUtilsService.DrivesUpdated += OnDrivesUpdated;

            SelectedScanningTargetIndex = 0;
            IsScanningNow = false;
        }

        // Вызывается для отправки нового буфера отсканированной информации, чтобы через Dispatcher добавить в UI
        private void OnDrivesUpdated(DirectoryNode node, List<FileSystemNode>? list, long size)
        {
            DispatcherQueueProvider.DispatcherQueue.TryEnqueue(() =>
            {
                if(list != null && list.Any())
                {
                    foreach(FileSystemNode item in list)
                    {
                        if(item is DirectoryNode directoryNode)
                        {
                            node.FileSystemNodes.Add(directoryNode);
                        }
                        else if (item is FileNode fileNode)
                        {
                            node.FileSystemNodes.Add(fileNode);
                        }
                    }
                }

                node.Size += size;
            });
        }

        #region Properties

        private CancellationTokenSource? CurrentScanningCancellationTokenSource { get; set; } // Для отмены
        private PauseResetTokenSource PauseResetTokenSource { get; set; } // Для паузы/возобновления

        /// <summary>
        /// Главная коллекция, содержит перечень дисков со всеми сопутствующими вложениями
        /// </summary>
        public ObservableCollection<DirectoryNode> DriveNodes { get; set; }
        /// <summary>
        /// Все доступные диски на устройстве.
        /// </summary>
        public ObservableCollection<DriveInfo> AllAvailableDrives { get; set; }
        /// <summary>
        /// Выбранные диски среди доступных.
        /// </summary>
        public ObservableCollection<DriveInfo> SelectedTargetDrives { get; set; }

        // Режим выбора: Все диски или выбранные
        private int _selectedScanningTargetIndex;
        public int SelectedScanningTargetIndex
        {
            get { return _selectedScanningTargetIndex; }
            set { SetProperty(ref _selectedScanningTargetIndex, value); }
        }

        // Выбранная директория в визуальном дереве.
        private DirectoryNode? _selectedDirectoryNodeNode;
        public DirectoryNode? SelectedDirectoryNode
        {
            get { return _selectedDirectoryNodeNode; }
            set { SetProperty(ref _selectedDirectoryNodeNode, value); }
        }

        private bool _isScanningNow;
        public bool IsScanningNow
        {
            get { return _isScanningNow; }
            set 
            { 
                if(SetProperty(ref _isScanningNow, value))
                {
                    (OpenTargetSelectDialogCommand as RelayCommand<XamlRoot>)!.NotifyCanExecuteChanged();
                    (RefreshScanningCommand as RelayCommand<XamlRoot>)!.NotifyCanExecuteChanged();
                    (RescanSelectedDirectoryCommand as RelayCommand)!.NotifyCanExecuteChanged();
                    (CancelScanningCommand as RelayCommand<XamlRoot>)!.NotifyCanExecuteChanged();
                    (ResumeScanningCommand as RelayCommand)!.NotifyCanExecuteChanged();
                    (PauseScanningCommand as RelayCommand)!.NotifyCanExecuteChanged();
                }
            }
        }

        private bool _isPausedNow;
        public bool IsPausedNow
        {
            get { return _isPausedNow; }
            set
            {
                if(SetProperty(ref _isPausedNow, value))
                {
                    (ResumeScanningCommand as RelayCommand)!.NotifyCanExecuteChanged();
                    (PauseScanningCommand as RelayCommand)!.NotifyCanExecuteChanged();
                }
            }
        }
        #endregion

        #region Commands

        // Открывает диалог выбора целей для сканирования (дисков)
        private ICommand? _openTargetSelectDialogCommand;
        public ICommand OpenTargetSelectDialogCommand => _openTargetSelectDialogCommand ??= new RelayCommand<XamlRoot>(async (xamlRoot) =>
        {
            LoadAvailableDrives();

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

                    foreach (DriveInfo drive in SelectedTargetDrives)
                    {
                        DriveNodes.Add(new DirectoryNode(drive.Name, drive.RootDirectory.FullName, 0, drive.RootDirectory.LastWriteTime));

                        await ScanSelectedTarget(DriveNodes, CurrentScanningCancellationTokenSource, PauseResetTokenSource);
                    }
                }
                else
                {
                    DriveNodes.Clear();

                    CurrentScanningCancellationTokenSource = new CancellationTokenSource();

                    foreach (DriveInfo drive in AllAvailableDrives)
                    {
                        DriveNodes.Add(new DirectoryNode(drive.Name, drive.RootDirectory.FullName, 0, drive.RootDirectory.LastWriteTime));
                        await ScanSelectedTarget(DriveNodes, CurrentScanningCancellationTokenSource, PauseResetTokenSource);
                    }
                }
            }
            
        }, (xamltoor) => !IsScanningNow);


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
            var dialogResult = await DialogManager.ShowContentDialog(xamlRoot!, "Rescan target confirmation", "Confirm",
               ContentDialogButton.Primary, $"Are you sure you want to rescan the following count of drives: " +
               $"{(SelectedScanningTargetIndex == 1 ? SelectedTargetDrives.Count : AllAvailableDrives.Count)}?", "Cancel", null);

            if(dialogResult == ContentDialogResult.Primary)
            {
                foreach(DirectoryNode drive in DriveNodes)
                {
                    drive.FileSystemNodes.Clear();
                }

                if (CurrentScanningCancellationTokenSource != null)
                    CurrentScanningCancellationTokenSource.Dispose();

                CurrentScanningCancellationTokenSource = new CancellationTokenSource();

                await ScanSelectedTarget(DriveNodes, CurrentScanningCancellationTokenSource, PauseResetTokenSource);
            }
        }, (xamlRoot) => !IsScanningNow);


        // Запускает повторное сканирование для выбранной директории
        private ICommand? _rescanSelectedDirectoryCommand;
        public ICommand RescanSelectedDirectoryCommand => _rescanSelectedDirectoryCommand ??= new RelayCommand(async () =>
        {
            if(SelectedDirectoryNode != null)
            {
                if (IsScanningNow || IsPausedNow)
                    return;

                if (CurrentScanningCancellationTokenSource != null)
                    CurrentScanningCancellationTokenSource.Dispose();

                CurrentScanningCancellationTokenSource = new CancellationTokenSource();

                SelectedDirectoryNode.FileSystemNodes.Clear();

                await ScanSelectedTarget(SelectedDirectoryNode, CurrentScanningCancellationTokenSource, PauseResetTokenSource);

            }

        }, () => !IsScanningNow);


        #region Scanning managing commands

        // Отменяет процесс сканирование
        private ICommand? _cancelScanningCommand;
        public ICommand CancelScanningCommand => _cancelScanningCommand ??= new RelayCommand<XamlRoot>(async (xamlRoot) =>
        {

            var dialogResult = await DialogManager.ShowContentDialog(xamlRoot!, "Cancel scanning confirmation", "Yes",
                ContentDialogButton.Primary, $"Are you sure you want to cancel the scanning process?", "No", null);

            if (dialogResult == ContentDialogResult.Primary)
            {
                if (CurrentScanningCancellationTokenSource == null)
                    return;

                CurrentScanningCancellationTokenSource.Cancel();
            }    
        }, (xamlRoot) => IsScanningNow);

        // Возобновляет процесс сканирования после паузы
        private ICommand? _resumeScanningCommand;
        public ICommand ResumeScanningCommand => _resumeScanningCommand ??= new RelayCommand(async () =>
        {
            PauseResetTokenSource.Reset();
            IsPausedNow = false;

        }, () => IsScanningNow && IsPausedNow);

        // Ставит процесс сканирования на паузу
        private ICommand? _pauseScanningCommand;
        public ICommand PauseScanningCommand => _pauseScanningCommand ??= new RelayCommand(async () =>
        {
            PauseResetTokenSource.Pause();
            IsPausedNow = true;

        }, () => IsScanningNow && !IsPausedNow);

        // Устанавливает выбранную директорию
        /*
         Поскольку привязка коллекции к TreeView отсутствует, выбранным элементом при прямой привязке стал бы TreeViewItem,
         поэтому в code behind главной страницы вручную обрабатывается событие изменения и передает информацию сюда.
         */
        private ICommand? _fileSystemNodeSelectionChanged;
        public ICommand FileSystemNodeSelectionChanged => _fileSystemNodeSelectionChanged ??= new RelayCommand<TreeViewNode?>(async (node) =>
        {
            if(node == null)
            {
                SelectedDirectoryNode = null;
                return;
            }

            if (node.Content is DirectoryNode directoryNode)
            {
                SelectedDirectoryNode = directoryNode;
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
        private async Task ScanSelectedTarget<T>(T target, CancellationTokenSource cts, PauseResetTokenSource prts)
        {
            IsScanningNow = true;

            if (target is DirectoryNode directoryNode)
            {
                await DriveUtilsService.ScanSpecifiedDirectory(directoryNode, cts.Token, prts.Token);
            }
            else if (target is ObservableCollection<DirectoryNode> collection)
            {
                await DriveUtilsService.ScanProvidedDrives(collection, cts.Token, prts.Token);
            }
            
            IsScanningNow = false;

            if(CurrentScanningCancellationTokenSource != null)
                CurrentScanningCancellationTokenSource.Dispose();
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
        #endregion
    }
}
