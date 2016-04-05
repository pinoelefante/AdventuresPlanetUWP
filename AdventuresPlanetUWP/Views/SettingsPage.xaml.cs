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
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        public OpzioniPageViewModel VM => this.DataContext as OpzioniPageViewModel;

        private void cancella_selezionati(object sender, RoutedEventArgs e)
        {
            bool news = (bool)del_news.IsChecked;
            bool rece = (bool)del_rece.IsChecked;
            bool solu = (bool)del_solu.IsChecked;
            bool podc = (bool)del_podcast.IsChecked;
            if(news || rece || solu || podc)
                VM.cancella_dati_sel(news, rece, solu, podc);
        }
    }
}
