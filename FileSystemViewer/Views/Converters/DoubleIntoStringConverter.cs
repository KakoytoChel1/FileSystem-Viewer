using Microsoft.UI.Xaml.Data;
using System;

namespace FileSystemViewer.Views.Converters
{
    public class DoubleIntoStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(value is double percent)
            {
                return $"{(percent).ToString("F2")}%";
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
