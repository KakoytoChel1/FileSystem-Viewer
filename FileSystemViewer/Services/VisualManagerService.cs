using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Services.Interfaces;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.UI;
using WinRT.Interop;

namespace FileSystemViewer.Services
{
    public class VisualManagerService : IVisualManagerService
    {
        private Window? _mainWindow;

        public void Initialize(Window mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void SetAccentColor(Color newColor)
        {
            var res = Application.Current.Resources;

            res["SystemAccentColorDark1"] = newColor;
            res["SystemAccentColorDark2"] = newColor;
            res["SystemAccentColorDark3"] = newColor;
            res["SystemAccentColorLight1"] = newColor;
            res["SystemAccentColorLight2"] = newColor;
            res["SystemAccentColorLight3"] = newColor;

            RefreshUI();
        }

        public void SetApplicationTheme(AppSettings.ThemeMode themeMode)
        {
            if (_mainWindow != null && _mainWindow.Content is FrameworkElement root)
            {
                switch (themeMode)
                {
                    case AppSettings.ThemeMode.Light:
                        root.RequestedTheme = ElementTheme.Light;
                        break;
                    case AppSettings.ThemeMode.Dark:
                        root.RequestedTheme = ElementTheme.Dark;
                        break;
                    case AppSettings.ThemeMode.System:
                        root.RequestedTheme = ElementTheme.Default;
                        break;
                }
                UpdateTitleBarColors(root.ActualTheme);
            }
        }

        private void RefreshUI()
        {
            // Refreshing theme to force the UI to update its resources
            if (_mainWindow != null && _mainWindow.Content is FrameworkElement root)
            {
                var currentTheme = root.RequestedTheme;
                root.RequestedTheme = currentTheme == ElementTheme.Light ? ElementTheme.Dark : ElementTheme.Light;
                root.RequestedTheme = currentTheme;
            }
        }

        public void UpdateTitleBarColors(ElementTheme currentTheme)
        {
            var hWnd = WindowNative.GetWindowHandle(_mainWindow);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            if (AppWindowTitleBar.IsCustomizationSupported() && appWindow.TitleBar != null)
            {
                var titleBar = appWindow.TitleBar;

                Color transparent = Colors.Transparent;
                bool isDark = currentTheme == ElementTheme.Dark;
                Color foreground = isDark ? Colors.White : Colors.Black;

                Color hoverBackground = isDark ? Color.FromArgb(25, 255, 255, 255) : Color.FromArgb(25, 0, 0, 0);
                Color pressedBackground = isDark ? Color.FromArgb(51, 255, 255, 255) : Color.FromArgb(51, 0, 0, 0);
                Color inactiveForeground = isDark ? Color.FromArgb(100, 255, 255, 255) : Color.FromArgb(100, 0, 0, 0);

                titleBar.ButtonBackgroundColor = transparent;
                titleBar.ButtonForegroundColor = foreground;

                titleBar.ButtonHoverBackgroundColor = hoverBackground;
                titleBar.ButtonHoverForegroundColor = foreground;

                titleBar.ButtonPressedBackgroundColor = pressedBackground;
                titleBar.ButtonPressedForegroundColor = foreground;

                titleBar.ButtonInactiveBackgroundColor = transparent;
                titleBar.ButtonInactiveForegroundColor = inactiveForeground;
            }
        }
    }
}
