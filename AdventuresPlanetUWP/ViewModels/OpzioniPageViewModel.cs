﻿using AdventuresPlanetUWP.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
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
        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            return base.OnNavigatedToAsync(parameter, mode, state);
        }
        private ResourceLoader res = ResourceLoader.GetForCurrentView("Settings");
        public async void cancella_dati(object sender, object e)
        {
            MessageDialog dlg = new MessageDialog(res.GetString("settings_cancellazione_messaggio"),res.GetString("settings_cancellazione_titolo"));
            UICommand del = new UICommand(res.GetString("settings_cancellazione_conferma")) { Id = 0 };
            del.Invoked = (x) => 
            {
                DatabaseSystem.Instance.dropDB();
                Settings.LastNewsUpdate = 0;
                Settings.LastPodcastUpdate = 0;
                Settings.LastRecensioniUpdate = 0;
                Settings.LastSoluzioniUpdate = 0;
                Settings.LastGallerieUpdate = 0;
                AdventuresPlanetManager.Instance.Reset(true, true, true, true);
            };
            UICommand annulla = new UICommand(res.GetString("settings_cancellazione_annulla")) { Id = 1 };
            dlg.Commands.Add(del);
            dlg.Commands.Add(annulla);
            dlg.DefaultCommandIndex = 0;
            dlg.CancelCommandIndex = 1;

            await dlg.ShowAsync();
        }
        public async void cancella_dati_sel(bool news, bool rece, bool solu, bool podc, bool gall)
        {
            MessageDialog dlg = new MessageDialog("Vuoi cancellare i dati selezionati?", "Conferma cancellazione");
            UICommand del = new UICommand("Si") { Id = 0 };
            del.Invoked = (x) =>
            {
                if (news)
                {
                    DatabaseSystem.Instance.cleanNews();
                    AdventuresPlanetManager.Instance.ListaNews?.Reset();
                    Settings.LastNewsUpdate = 0;
                }
                if (rece)
                {
                    DatabaseSystem.Instance.cleanRecensioni();
                    Settings.LastRecensioniUpdate = 0;
                }
                if (solu)
                {
                    DatabaseSystem.Instance.cleanSoluzioni();
                    Settings.LastSoluzioniUpdate = 0;
                    
                }
                if (podc)
                {
                    DatabaseSystem.Instance.cleanPodcast();
                    Settings.LastPodcastUpdate = 0;
                }
                if (gall)
                {
                    DatabaseSystem.Instance.cleanGallerie();
                    Settings.LastGallerieUpdate = 0;
                }
                AdventuresPlanetManager.Instance.Reset(news, rece, solu, podc);
                DatabaseSystem.Instance.vacuum();
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
