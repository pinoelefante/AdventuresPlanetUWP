using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace AdventuresPlanetUWP.Converters
{
    class NotBoolToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool v = System.Convert.ToBoolean(value);
            return v ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            Visibility v = (Visibility)value;
            return v == Visibility.Collapsed ? true : false;
        }
    }
}
