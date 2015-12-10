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
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;

namespace AdventuresPlanetUWP.ViewModels
{
    public class RecensioniPageViewModel : Mvvm.ViewModelBase
    {

        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            Debug.WriteLine("Recensioni on navigatedTo");
            if(ListaRecensioni==null || ListaRecensioni.Count == 0)
                GroupByLetter();
        }
        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            ListaRecensioni.Clear();
            ListaRecensioni = null;
            Debug.WriteLine("Recensioni on navigatedFromAsync");
            return base.OnNavigatedFromAsync(state, suspending);
        }
        public void GroupByLetter()
        {
            ListaRecensioni?.Clear();
            SelectedMode = ModalitaVisualizzazione.AlphaKey;
            ListaRecensioni = MyGrouping<RecensioneItem>.AlphaKeyGroup(AdventuresPlanetManager.Instance.ListaRecensioni, 
                                                                        (t => t.Titolo), 
                                                                        true);
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
        private CollectionViewSource _collection;
        public CollectionViewSource Collection
        {
            get
            {
                return _collection;
            }
            set
            {
                Set<CollectionViewSource>(ref _collection, value);
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
