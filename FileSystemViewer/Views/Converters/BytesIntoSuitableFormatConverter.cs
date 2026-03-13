using Humanizer;
using Microsoft.UI.Xaml.Data;
using System;

namespace FileSystemViewer.Views.Converters
{
    public class BytesIntoSuitableFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            long sizeInBytes = 0;
            
            sizeInBytes = (long)value;

            return sizeInBytes.Bytes().Humanize();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
