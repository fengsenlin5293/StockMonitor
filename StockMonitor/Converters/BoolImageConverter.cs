using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace StockMonitor.Converters
{
    public class BoolImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            bool suc = bool.TryParse(value.ToString(), out var result);
            if (!suc)
                return null;
            if (result)
                return new Uri("pack://application:,,,/Resource;component/Images/Common/success.png");
            else
                return new Uri("pack://application:,,,/Resource;component/Images/Common/failed.png");
                 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
