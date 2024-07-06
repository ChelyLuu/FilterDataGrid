using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FilterDataGrid.Converters
{
    public class VisibleToCheckConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value == null) return null;
            Visibility visibility = (Visibility)value;
            if (visibility == Visibility.Visible) return true; else return false;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
