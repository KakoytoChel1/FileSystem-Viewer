using CommunityToolkit.Mvvm.Input;
using FileSystem_Viewer.Models;
using FileSystem_Viewer.Services.IServices;
using FileSystem_Viewer.ViewModels.Tools;
using FileSystem_Viewer.Views.DialogPages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FileSystem_Viewer.ViewModels
{
    // * Исправить дивный баг с TemplateSelector
    // 1. Селектор дисков, реализовать +
    // 2. Сканирование выбранной директории реализовать
    // 3. Пересканирование целевых цисков реализовать
    // 4. * Блокировки UI во время сканирования, и доп информация в шаблоне TreeView реализовать.

    public class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel(IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider) : base(driveUtilsService, dispatcherQueueProvider)
        {
            DriveNodes = new ObservableCollection<DirectoryNode>();
            AvailableDrives = new ObservableCollection<DriveInfo>();
            SelectedTargetDrives = new ObservableCollection<DriveInfo>();

            DriveUtilsService.DrivesUpdated += OnDrivesUpdated;

            SelectedScanningTargetIndex = 0;
        }

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

        public ObservableCollection<DirectoryNode> DriveNodes { get; set; }
        /// <summary>
        /// Все доступные диски на устройстве.
        /// </summary>
        public ObservableCollection<DriveInfo> AvailableDrives { get; set; }
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
        #endregion

        #region Commands

        private ICommand? _openTargetSelectDialogCommand;
        public ICommand OpenTargetSelectDialogCommand => _openTargetSelectDialogCommand ??= new RelayCommand<XamlRoot>(async (xamlRoot) =>
        {
            if (xamlRoot == null)
                return;

            LoadAvailableDrives();

            var dialogResult = await DialogManager.ShowContentDialog(xamlRoot, "Target selection...", "Apply",
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

                    foreach (DriveInfo drive in SelectedTargetDrives)
                    {
                        DriveNodes.Add(new DirectoryNode(drive.Name, drive.RootDirectory.FullName, 0, drive.RootDirectory.LastWriteTime));

                        await ScanSelectedTarget();
                    }
                }
                else
                {
                    DriveNodes.Clear();

                    foreach (DriveInfo drive in AvailableDrives)
                    {
                        DriveNodes.Add(new DirectoryNode(drive.Name, drive.RootDirectory.FullName, 0, drive.RootDirectory.LastWriteTime));
                        await ScanSelectedTarget();
                    }
                }
            }
            
        });


        private ICommand? _refreshAvailableDrivesCollectionCommand;
        public ICommand RefreshAvailableDrivesCollectionCommand => _refreshAvailableDrivesCollectionCommand ??= new RelayCommand(() =>
        {
            LoadAvailableDrives();
        });

        private ICommand? _scanCommand;
        public ICommand ScanCommand => _scanCommand ??= new RelayCommand(async () =>
        {
            
        });
        #endregion

        #region Methods

        private async Task ScanSelectedTarget()
        {
            await DriveUtilsService.ScanProvidedDrives(DriveNodes, CancellationToken.None);

            var size = DriveNodes.FirstOrDefault()?.Size ?? 0;

            AppNotification notification = new AppNotificationBuilder()
                .AddText("Scanning operation completed!")
                .AddText("The scanning of your targets has been successfully completed!")
                .BuildNotification();

            AppNotificationManager.Default.Show(notification);
        }

        private void LoadAvailableDrives()
        {
            AvailableDrives.Clear();
            var drives = DriveUtilsService.GetAvailableDrives();
            foreach (var drive in drives)
            {
                AvailableDrives.Add(drive);
            }
        }
        #endregion
    }
}
