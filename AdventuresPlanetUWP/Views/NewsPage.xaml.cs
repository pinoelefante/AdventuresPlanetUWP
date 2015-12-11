using AdventuresPlanetUWP.Classes;
using AdventuresPlanetUWP.Classes.Data;
using AdventuresPlanetUWP.Converters;
using AdventuresPlanetUWP.ViewModels;
using AdventuresPlanetUWP.Views.UserControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// Il modello di elemento Pagina vuota è documentato all'indirizzo http://go.microsoft.com/fwlink/?LinkId=234238

namespace AdventuresPlanetUWP.Views
{
    /// <summary>
    /// Pagina vuota che può essere usata autonomamente oppure per l'esplorazione all'interno di un frame.
    /// </summary>
    public sealed partial class NewsPage : Page
    {
        public NewsPage()
        {
            this.InitializeComponent();
            this.Loaded += (s, e) =>
            {
                if (ChristmasTime.IsChristmasTime())
                    snowflakes = ChristmasTime.LetItSnow(LayoutRoot);
            };
            this.Unloaded += (s, e) =>
            {
                if (snowflakes != null)
                {
                    snowflakes.Stop();
                }
            };
        }
        private DispatcherTimer snowflakes;
        public NewsPageViewModel VM => this.DataContext as NewsPageViewModel;

        private void showAnteprima(object sender, RoutedEventArgs e)
        {
            News n = (sender as FrameworkElement).DataContext as News;
            VM.showAnteprima(n);
        }
        private void openNews(object sender, TappedRoutedEventArgs e)
        {
            VM.openNews((sender as FrameworkElement).DataContext as News);
        }

        private void openFlyout(object sender, object e)
        {
            Flyout.ShowAttachedFlyout(sender as FrameworkElement);
        }
    }
}
