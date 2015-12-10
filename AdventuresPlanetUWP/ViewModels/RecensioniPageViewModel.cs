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
using Windows.UI.Xaml.Navigation;

namespace AdventuresPlanetUWP.ViewModels
{
    public class RecensioniPageViewModel : Mvvm.ViewModelBase
    {

        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            GroupByLetter();
        }
        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            Debug.WriteLine("Recensioni on navigatedFromAsync");
            ListaRecensioni?.Clear();
            ListaRecensioni = null;
            return base.OnNavigatedFromAsync(state, suspending);
        }
        public void GroupByLetter()
        {
            ListaRecensioni?.Clear();
            SelectedMode = ModalitaVisualizzazione.AlphaKey;
            List<RecensioneItem> list = AdventuresPlanetManager.Instance.ListaRecensioni;
            ListaRecensioni = MyGrouping<RecensioneItem>.AlphaKeyGroup(list.ToList(), (t => t.Titolo), true);
            list.Clear();
            list = null;
        }
        public async void AggiornaRecensioni(object s, object e)
        {
            IsUpdatingRecensioni = true;
            bool res = await AdventuresPlanetManager.Instance.aggiornaRecensioni();
            if (res)
                GroupByLetter();
            IsUpdatingRecensioni = false;
        }
        private bool _isUpdating;
        public bool IsUpdatingRecensioni
        {
            get
            {
                return _isUpdating;
            }
            set
            {
                Set<bool>(ref _isUpdating, value);
            }
        } 
        private Dictionary<string, List<RecensioneItem>> _list;
        public Dictionary<string, List<RecensioneItem>> ListaRecensioni
        {
            get
            {
                return _list;
            }
            set
            {
                Set<Dictionary<string, List<RecensioneItem>>>(ref _list, value);
            }
        }
        private ModalitaVisualizzazione _selectedMode;
        public ModalitaVisualizzazione SelectedMode
        {
            get
            {
                return _selectedMode;
            }
            set
            {
                Set<ModalitaVisualizzazione>(ref _selectedMode, value);
            }
        }
        public void AddToPreferiti(RecensioneItem rec)
        {
            AdventuresPlanetManager.Instance.changeIsPreferita(rec.Id, true);
        }
        public void RemoveFromPreferiti(RecensioneItem rec)
        {
            AdventuresPlanetManager.Instance.changeIsPreferita(rec.Id, false);
        }
        public void Open(RecensioneItem item)
        {
            NavigationService.Navigate(typeof(ContentsPage), item);
        }
        public void GoToSearch(object s, object e)
        {
            NavigationService.Navigate(typeof(SearchPage), "Recensioni");
        }
    }
    public enum ModalitaVisualizzazione
    {
        AlphaKey,
        Voto
    }
}
