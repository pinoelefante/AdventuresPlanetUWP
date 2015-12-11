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
    public sealed partial class PodcastPage : Page
    {
        public PodcastPage()
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
        public PodcastPageViewModel VM => this.DataContext as PodcastPageViewModel;
    }
}
