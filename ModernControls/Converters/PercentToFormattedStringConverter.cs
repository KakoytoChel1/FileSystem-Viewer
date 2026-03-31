using Microsoft.UI.Xaml.Data;
using System;

namespace ModernControls.Converters
{
    public class PercentToFormattedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double percent)
            {
                return percent.ToString("F2");
            }

            return "0.00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
