using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace AdventuresPlanetUWP.Converters
{
    class IntToGridHeight : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int v = System.Convert.ToInt32(value);
            return v == 0 ? GridLength.Auto : new GridLength(1, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
