using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace FilterDataGrid.Converters
{
    public class ColumnVisibleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string c = values[1].ToString();
            if (values[0] == null || c == "") return false;
            else
            {
                ColumnProperty cp = (values[0] as List<ColumnProperty>).Where(x => x.Name == c).FirstOrDefault();
                if (cp.Visibility == Visibility.Visible) return true;
                else return false;
            }

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
