using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace StockMonitor.Converters
{
    public class BoolVisibilityConverter : IValueConverter
    {
        public bool IsTrueToVisible { get; set; }
        public object Convert(object sourceValue, Type targetType, object parameter, CultureInfo culture)
        {
            if (sourceValue == null)
                return Visibility.Collapsed;

            bool success = bool.TryParse(sourceValue.ToString(), out var value);
            if (!success)
                return Visibility.Collapsed;

            if (IsTrueToVisible && value)
            {
                return Visibility.Visible;
            }
            else if(!IsTrueToVisible && !value)
            {
                return Visibility.Visible;
            }
            else
                return Visibility.Collapsed;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
