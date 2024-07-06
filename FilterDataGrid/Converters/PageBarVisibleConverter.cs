using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FilterDataGrid.Converters
{
    public class PageBarVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value == null) return false;
            else
            {
                Visibility c = (Visibility)value;
                if (c == Visibility.Visible) return true;
                else return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
