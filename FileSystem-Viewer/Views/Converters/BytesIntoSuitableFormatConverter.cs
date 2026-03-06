using Humanizer;
using Microsoft.UI.Xaml.Data;
using System;

namespace FileSystem_Viewer.Views.Converters
{
    public class BytesIntoSuitableFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(value is long sizeInBytes)
            {
                return sizeInBytes.Bytes().Humanize();
            }

            return "0 B";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
