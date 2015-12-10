using AdventuresPlanetUWP.Classes;
using AdventuresPlanetUWP.Classes.Data;
using AdventuresPlanetUWP.Classes.Grouping;
using AdventuresPlanetUWP.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            LoadPreferiti();
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
            List<EntryAvventura> l = DatabaseSystem.Instance.selectAllPreferiti();
            ListaPreferiti = MyGrouping<EntryAvventura>.AlphaKeyGroup(l, x => { return x.Titolo; });
            IsEmpty = l.Count == 0;
            l.Clear();
        }
        public async void Open(EntryAvventura avv)
        {
            if (avv.RecensionePresente && avv.SoluzionePresente)
            {
                MessageDialog dlg = new MessageDialog("Vuoi aprire la recensione o la soluzione?", "Sono indeciso...");
                UICommand recensione = new UICommand("Recensione", (c) => { OpenRecensione(avv); }, 0);
                UICommand soluzione = new UICommand("Soluzione", (c) => { OpenSoluzione(avv); }, 1);
                //UICommand annulla = new UICommand("Annulla") { Id = 2 };
                dlg.Commands.Add(recensione);
                dlg.Commands.Add(soluzione);
                //dlg.Commands.Add(annulla);
                dlg.DefaultCommandIndex = 0;
                //dlg.CancelCommandIndex = 2;
                await dlg.ShowAsync();
            }
            else if (avv.SoluzionePresente)
                OpenSoluzione(avv);
            else
                OpenRecensione(avv);
        }
        public void OpenRecensione(EntryAvventura avv)
        {
            NavigationService.Navigate(typeof(ContentsPage), avv.Recensione);
        }
        public void OpenSoluzione(EntryAvventura avv)
        {
            NavigationService.Navigate(typeof(ContentsPage), avv.Soluzione);
        }
    }
}
