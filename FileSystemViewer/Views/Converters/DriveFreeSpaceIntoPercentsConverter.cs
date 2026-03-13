using FileSystemViewer.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;

namespace FileSystemViewer.Views.Converters
{
    public class DriveFreeSpaceIntoPercentsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DriveNode driveNode)
            {
                double result = (double)driveNode.TotalFreeSpace / driveNode.TotalSize;
                return $"({result:P0})";
            }
            return "(0%)";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
