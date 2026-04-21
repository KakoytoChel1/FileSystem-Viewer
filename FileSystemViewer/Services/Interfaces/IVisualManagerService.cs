using FileSystemViewer.Models.DataModels;
using Microsoft.UI.Xaml;
using Windows.UI;

namespace FileSystemViewer.Services.Interfaces
{
    public interface IVisualManagerService
    {
        public void Initialize(Window mainWindow);
        public void SetAccentColor(Color newColor);
        public void SetApplicationTheme(AppSettings.ThemeMode themeMode);
        public void SetWindowTheme(Window? window, AppSettings.ThemeMode themeMode);
        public void UpdateTitleBarColors(ElementTheme currentTheme, Window window);
    }
}
