﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace AdventuresPlanetUWP.Converters
{
    class IntToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int v = System.Convert.ToInt32(value);
            return v == 0 ? false : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
