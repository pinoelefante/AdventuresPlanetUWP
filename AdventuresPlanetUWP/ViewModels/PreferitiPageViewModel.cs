using AdventuresPlanetUWP.Classes;
using AdventuresPlanetUWP.Classes.Data;
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

        public List<EntryAvventura> ListaPreferiti { get; set; } 
        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            ListaPreferiti = DatabaseSystem.Instance.selectAllPreferiti();
            IsEmpty = ListaPreferiti.Count == 0;
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
            ListaPreferiti.Remove(avv);
            IsEmpty = ListaPreferiti.Count == 0;
        }
        public async void Open(EntryAvventura avv)
        {
            if (avv.RecensionePresente && avv.SoluzionePresente)
            {
                MessageDialog dlg = new MessageDialog("Vuoi aprire la recensione o la soluzione?", "Sono indeciso...");
                UICommand recensione = new UICommand("Recensione", (c) => { OpenRecensione(avv); }, 0);
                UICommand soluzione = new UICommand("Soluzione", (c) => { OpenSoluzione(avv); }, 1);
                dlg.Commands.Add(recensione);
                dlg.Commands.Add(soluzione);

                dlg.DefaultCommandIndex = 0;

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
