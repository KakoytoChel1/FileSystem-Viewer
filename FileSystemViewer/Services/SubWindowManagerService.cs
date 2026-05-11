using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Services.Interfaces;
using FileSystemViewer.ViewModels;
using FileSystemViewer.Views.Windows;
using System;

namespace FileSystemViewer.Services
{
    public class SubWindowManagerService : ISubWindowManagerService
    {
        private AppState _appState;
        private IConfigurationService<AppSettings> _configurationService;
        private IVisualManagerService _visualManagerService;
        private IServiceProvider _serviceProvider;

        public SubWindowManagerService(IServiceProvider serviceProvider, IConfigurationService<AppSettings> configurationService,
            IVisualManagerService visualManagerService, AppState appState)
        {
            _serviceProvider = serviceProvider;
            _configurationService = configurationService;
            _visualManagerService = visualManagerService;
            _appState = appState;
        }

        public void OpenTreeViewSubWindow()
        {
            string windowKey = nameof(TreeViewWindow);

            if (!_appState.ActiveSubWindows.ContainsKey(windowKey))
            {
                TreeViewWindow treeViewWindow = new TreeViewWindow(_serviceProvider!);
                _visualManagerService.SetWindowTheme(treeViewWindow, _configurationService.Settings!.AppTheme);
                treeViewWindow.Closed += (s, e) => _appState.ActiveSubWindows.Remove(windowKey);
                _appState.ActiveSubWindows.Add(windowKey, treeViewWindow);
                treeViewWindow.Activate();
            }
        }

        public void OpenTreemapSubWindow()
        {
            string windowKey = nameof(TreemapWindow);

            if (!_appState.ActiveSubWindows.ContainsKey(windowKey))
            {
                TreemapWindow treemapWindow = new TreemapWindow(_serviceProvider!);
                _visualManagerService.SetWindowTheme(treemapWindow, _configurationService.Settings!.AppTheme);
                treemapWindow.Closed += (s, e) => _appState.ActiveSubWindows.Remove(windowKey);
                _appState.ActiveSubWindows.Add(windowKey, treemapWindow);
                treemapWindow.Activate();
            }
        }

        public void OpenExtensionChartSubWindow()
        {
            string windowKey = nameof(ChartTabsWindow);

            if (!_appState.ActiveSubWindows.ContainsKey(windowKey))
            {
                ChartTabsWindow chartTabsWindow = new ChartTabsWindow(_serviceProvider!);
                _visualManagerService.SetWindowTheme(chartTabsWindow, _configurationService.Settings!.AppTheme);
                chartTabsWindow.Closed += (s, e) => _appState.ActiveSubWindows.Remove(windowKey);
                _appState.ActiveSubWindows.Add(windowKey, chartTabsWindow);
                chartTabsWindow.Activate();
            }
        }
    }
}
