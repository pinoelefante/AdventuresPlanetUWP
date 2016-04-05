using AdventuresPlanetUWP.Classes;
using AdventuresPlanetUWP.Classes.Data;
using AdventuresPlanetUWP.Classes.Grouping;
using AdventuresPlanetUWP.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.System.Profile;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;

namespace AdventuresPlanetUWP.ViewModels
{
    public class PreferitiPageViewModel : Mvvm.ViewModelBase
    {
        private Dictionary<string, List<EntryAvventura>> _pref;
        public Dictionary<string, List<EntryAvventura>> ListaPreferiti
        {
            get
            {
                return _pref;
            }
            private set
            {
                Set<Dictionary<string, List<EntryAvventura>>>(ref _pref, value);
            }
        }
        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            LoadPreferiti();
            return Task.CompletedTask;
        }
        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            ListaPreferiti?.Clear();
            ListaPreferiti = null;
            return base.OnNavigatedFromAsync(state, suspending);
        }
        private bool _isEmpty;
        public bool IsEmpty
        {
            get
            {
                return _isEmpty;
            }
            set
            {
                Set<bool>(ref _isEmpty, value);
            }
        }
        public void RimuoviPreferiti(EntryAvventura avv)
        {
            AdventuresPlanetManager.Instance.changeIsPreferita(avv.Id, false);
            LoadPreferiti();
        }
        private void LoadPreferiti()
        {
            ListaPreferiti?.Clear();
            var preferiti = PreferitiManager.Instance.GetPreferiti();
            var listEntry = new List<EntryAvventura>();
            foreach(var idPref in preferiti)
            {
                RecensioneItem rec = DatabaseSystem.Instance.selectRecensione(idPref);
                SoluzioneItem sol = DatabaseSystem.Instance.selectSoluzione(idPref);
                EntryAvventura avv = new EntryAvventura(rec, sol);
                listEntry.Add(avv);
            }
            ListaPreferiti = MyGrouping<EntryAvventura>.AlphaKeyGroup(listEntry, x => x.Titolo, true);
            IsEmpty = listEntry.Count == 0;
            listEntry.Clear();
            listEntry = null;
        }
        private ResourceLoader res = ResourceLoader.GetForCurrentView("Preferiti");
        public async void Open(EntryAvventura avv)
        {
            AnalyticsVersionInfo ai = AnalyticsInfo.VersionInfo;
            string family = ai.DeviceFamily;

            Debug.WriteLine("Family = " + family);
            if (avv.RecensionePresente && avv.SoluzionePresente)
            {
                MessageDialog dlg = new MessageDialog(res.GetString("preferiti_apertura_messaggio"), res.GetString("preferiti_apertura_titolo"));
                UICommand recensione = new UICommand(res.GetString("preferiti_apertura_rece"), (c) => { OpenRecensione(avv); }, 0);
                UICommand soluzione = new UICommand(res.GetString("preferiti_apertura_solu"), (c) => { OpenSoluzione(avv); }, 1);
                dlg.Commands.Add(recensione);
                dlg.Commands.Add(soluzione);
                dlg.DefaultCommandIndex = 0;

                if (family.Equals("Windows.Desktop"))
                {
                    UICommand annulla = new UICommand(res.GetString("preferiti_apertura_annulla")) { Id = 2 };
                    dlg.Commands.Add(annulla);
                    dlg.CancelCommandIndex = 2;
                }
                await dlg.ShowAsync();
            }
            else if (avv.SoluzionePresente)
                OpenSoluzione(avv);
            else
                OpenRecensione(avv);
        }
        public void OpenRecensione(EntryAvventura avv)
        {
            NavigationService.Navigate(typeof(ViewRecensione), avv.Recensione);
        }
        public void OpenSoluzione(EntryAvventura avv)
        {
            NavigationService.Navigate(typeof(ViewSoluzione), avv.Soluzione);
        }
        public void BackupPreferiti(object s = null, object e = null)
        {
            PreferitiManager.Instance.BackupPreferiti();
        }
        public async void RecoverBackup(object s = null, object e = null)
        {
            bool res = await PreferitiManager.Instance.RecoverPreferitiFromBackup();
            if(res)
                LoadPreferiti();
        }
        public async void RemoveAll(object s = null, object e = null)
        {
            MessageDialog dlg = new MessageDialog("Vuoi eliminare tutti i preferiti?", "Conferma");
            UICommand yes = new UICommand()
            {
                Id = 0,
                Label = "Si",
                Invoked =
                (x) =>
                {
                    PreferitiManager.Instance.RemoveAllPreferiti();
                    ListaPreferiti.Clear();
                    ListaPreferiti = null;
                }
            };
            UICommand no = new UICommand() { Id = 1, Label = "No" };
            dlg.Commands.Add(yes);
            dlg.Commands.Add(no);
            dlg.CancelCommandIndex = 1;
            dlg.DefaultCommandIndex = 0;
            await dlg.ShowAsync();
        }
    }
}
