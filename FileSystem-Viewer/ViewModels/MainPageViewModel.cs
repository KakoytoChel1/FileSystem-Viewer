using CommunityToolkit.Mvvm.Input;
using FileSystem_Viewer.Models;
using FileSystem_Viewer.Services.IServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;

namespace FileSystem_Viewer.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel(IDriveUtilsService driveUtilsService, IDispatcherQueueProvider dispatcherQueueProvider) : base(driveUtilsService, dispatcherQueueProvider)
        {
            StartButtonText = "Start Scanning";

            DiskNodes = new ObservableCollection<DirectoryNode>();
            DriveUtilsService.DrivesUpdated += OnDrivesUpdated;

            var availableDrives = DriveUtilsService.GetAvailableDrives();
            foreach (var drive in availableDrives)
            {
                DiskNodes.Add(new DirectoryNode
                {
                    Name = drive.Name,
                    FullPath = drive.Name,
                    Size = 0,
                    FileSystemNodes = new ObservableCollection<FileSystemNode>()
                });
            }
        }

        private void OnDrivesUpdated(DirectoryNode node, List<FileSystemNode>? list, long size)
        {
            DispatcherQueueProvider.DispatcherQueue.TryEnqueue(() =>
            {
                if(list != null)
                {
                    foreach(var item in list)
                    {
                        node.FileSystemNodes.Add(item);
                    }
                }

                node.Size += size;
            });
        }

        #region Properties

        public ObservableCollection<DirectoryNode> DiskNodes { get; set; }

        private string? _startButtonText;
        public string? StartButtonText
        {
            get => _startButtonText;
            set
            {
                if (_startButtonText != value)
                {
                    _startButtonText = value;
                    OnPropertyChanged(nameof(StartButtonText));
                }
            }
        }
        #endregion

        #region Commands

        private ICommand? _scanCommand;
        public ICommand ScanCommand => _scanCommand ??= new RelayCommand(async () =>
        {
            StartButtonText = "Scanning...";
            await DriveUtilsService.ScanProvidedDrives(DiskNodes, CancellationToken.None);

            StartButtonText = "Scanning completed";
        });
        #endregion
    }
}
