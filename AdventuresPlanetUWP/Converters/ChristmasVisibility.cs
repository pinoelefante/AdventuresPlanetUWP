using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace AdventuresPlanetUWP.Converters
{
    class ChristmasVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DateTime now = DateTime.Now;
            int anno = now.Date.Year;
            int mese = now.Month;
            if (mese != 12 && mese != 1)
                return Visibility.Collapsed;

            int anno_end = mese == 12 ? anno + 1 : anno;

            int anno_begin = mese == 12 ? anno : anno - 1;

            DateTime end = new DateTime(anno_end, 1, 7);
            DateTime begin = new DateTime(anno_begin, 12, 8);

            if (now.Ticks > begin.Ticks && now.Ticks < end.Ticks)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
