using AdventuresPlanetUWP.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace AdventuresPlanetUWP.Converters
{
    class WidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            ModalitaVisualizzazione mod = (ModalitaVisualizzazione)value;
            Frame frame = Template10.Common.WindowWrapper.Current().Window.Content as Frame;
            if (mod == ModalitaVisualizzazione.Autore)
            {
                return (int)(frame.Width - 20);
            }
            else
            {
                return 64;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
