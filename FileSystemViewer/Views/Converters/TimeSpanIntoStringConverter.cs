using Humanizer;
using Microsoft.UI.Xaml.Data;
using System;

namespace FileSystemViewer.Views.Converters
{
    public class TimeSpanIntoStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan timeSpan)
            {
                return timeSpan.Humanize();
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
