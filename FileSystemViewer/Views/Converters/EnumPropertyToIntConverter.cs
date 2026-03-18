using FileSystemViewer.ViewModels;
using Microsoft.UI.Xaml.Data;
using System;

namespace FileSystemViewer.Views.Converters
{
    public class EnumPropertyToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is MainPageViewModel.ScanningStates state)
            {
                return (int)state;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
