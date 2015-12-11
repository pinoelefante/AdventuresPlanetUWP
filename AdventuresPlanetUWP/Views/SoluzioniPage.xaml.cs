using AdventuresPlanetUWP.Classes.Data;
using AdventuresPlanetUWP.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace AdventuresPlanetUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SoluzioniPage : Page
    {
        public SoluzioniPage()
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
        public SoluzioniPageViewModel VM => this.DataContext as SoluzioniPageViewModel;
        private void AddPreferiti(object sender, RoutedEventArgs e)
        {
            SoluzioneItem item = (sender as FrameworkElement).DataContext as SoluzioneItem;
            VM.AddPreferiti(item);
        }

        private void DelPreferiti(object sender, RoutedEventArgs e)
        {
            SoluzioneItem item = (sender as FrameworkElement).DataContext as SoluzioneItem;
            VM.DelPreferiti(item);
        }

        private void openFlyout(object sender, object e)
        {
            Flyout.ShowAttachedFlyout(sender as FrameworkElement);
        }

        private void openSoluzione(object sender, TappedRoutedEventArgs e)
        {
            SoluzioneItem sol = (sender as FrameworkElement).DataContext as SoluzioneItem;
            VM.Open(sol);
        }
    }
}
