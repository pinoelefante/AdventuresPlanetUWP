using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace AdventuresPlanetUWP.Converters
{
    class StringToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string v = System.Convert.ToString(value);
            if (string.IsNullOrEmpty(v))
                return false;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
