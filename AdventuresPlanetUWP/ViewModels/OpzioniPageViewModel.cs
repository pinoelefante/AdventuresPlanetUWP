using AdventuresPlanetUWP.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace AdventuresPlanetUWP.ViewModels
{
    public class OpzioniPageViewModel : Mvvm.ViewModelBase
    {
        public Settings Settings
        {
            get
            {
                return Settings.Instance;
            }
        }
        public async void cancella_dati(object sender, object e)
        {
            MessageDialog dlg = new MessageDialog("Sei sicuro di voler cancellare tutti i dati?\nI preferiti verranno conservati","Cancella dati");
            UICommand del = new UICommand("Si") { Id = 0 };
            del.Invoked = (x) => 
            {
                Settings.LastMesiNewsUpdate = 0;
                DatabaseSystem.Instance.cleanTables();
                Settings.LastNewsUpdate = 0;
                Settings.LastPodcastUpdate = 0;
                Settings.LastRecensioniUpdate = 0;
                Settings.LastSoluzioniUpdate = 0;
                AdventuresPlanetManager.Instance.Reset();
            };
            UICommand annulla = new UICommand("Annulla") { Id = 1 };
            dlg.Commands.Add(del);
            dlg.Commands.Add(annulla);
            dlg.DefaultCommandIndex = 0;
            dlg.CancelCommandIndex = 1;

            await dlg.ShowAsync();
        }
        public void VideoQualityLoaded(object sender, object e)
        {
            int video = Settings.QualitaVideoMax;
            ComboBox combo = sender as ComboBox;
            foreach(ComboBoxItem item in combo.Items)
            {
                if (item.Tag.ToString().Equals(video.ToString()))
                {
                    combo.SelectedItem = item;
                    break;
                }
            }
        }
        public void VideoQualityChanged(object s, object e)
        {
            ComboBox combo = s as ComboBox;
            int qual = Int32.Parse(((ComboBoxItem)combo.SelectedItem).Tag.ToString());
            Settings.QualitaVideoMax = qual;
        }
        public void NewsTimeLoaded(object sender, object e)
        {
            long time = Settings.TimeUpdateNews;
            ComboBox combo = sender as ComboBox;
            foreach(ComboBoxItem item in combo.Items)
            {
                if (item.Tag.ToString().Equals(time.ToString()))
                {
                    combo.SelectedItem = item;
                    break;
                }
            }
        }
        public void NewsTimeUpdated(object sender, object e)
        {
            ComboBox combo = sender as ComboBox;
            long newTime = long.Parse((combo.SelectedItem as ComboBoxItem).Tag.ToString());
            Settings.TimeUpdateNews = newTime;
        }
    }
}
