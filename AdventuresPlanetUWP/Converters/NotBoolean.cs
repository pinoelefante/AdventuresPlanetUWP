using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace AdventuresPlanetUWP.Converters
{
    class NotBoolean : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool v = System.Convert.ToBoolean(value);
            return !v;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            bool v = System.Convert.ToBoolean(value);
            return !v;
        }
    }
}
