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
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            Debug.WriteLine("Recensioni on navigatedTo");
            if (!await Settings.Instance.IsRecensioniUpdated() && !Settings.Instance.UpdateRecensioniManualmente)
                AggiornaRecensioni();
            if(ListaRecensioni==null || ListaRecensioni.Count == 0)
                GroupByLetter();
            await base.OnNavigatedToAsync(parameter, mode, state);
        }
        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            Debug.WriteLine("Recensioni on navigatedFromAsync");
            return base.OnNavigatedFromAsync(state, suspending);
        }
        public void GroupByLetter(object s=null, object e=null)
        {
            if (SelectedMode == ModalitaVisualizzazione.AlphaKey && ListaRecensioni!=null && ListaRecensioni.Count > 0)
                return;
            ListaRecensioni?.Clear();
            SelectedMode = ModalitaVisualizzazione.AlphaKey;
            ListaRecensioni = MyGrouping<RecensioneItem>.AlphaKeyGroup(AdventuresPlanetManager.Instance.ListaRecensioni, 
                                                                        (t => t.Titolo), 
                                                                        true, 
                                                                        "", 
                                                                        true);
        }
        public void GroupByVoto(object s = null, object e = null)
        {
            if (SelectedMode == ModalitaVisualizzazione.Voto && ListaRecensioni != null && ListaRecensioni.Count > 0)
                return;
            ListaRecensioni?.Clear();
            SelectedMode = ModalitaVisualizzazione.Voto;
            ListaRecensioni = MyGrouping<RecensioneItem>.NumericKeyGroup(AdventuresPlanetManager.Instance.ListaRecensioni,
                                                                        (t => t.VotoInt),
                                                                        (t => t.Titolo),
                                                                        true,
                                                                        true);
        }
        public void GroupByAuthor(object s = null, object e = null)
        {
            if (SelectedMode == ModalitaVisualizzazione.Autore && ListaRecensioni != null && ListaRecensioni.Count > 0)
                return;
            ListaRecensioni?.Clear();
            SelectedMode = ModalitaVisualizzazione.Autore;
            ListaRecensioni = MyGrouping<RecensioneItem>.StringKeyGroup(AdventuresPlanetManager.Instance.ListaRecensioni,
                                                                        (t => t.AutoreText),
                                                                        true,
                                                                        "N.D.", 
                                                                        true);
        }
        public async void AggiornaRecensioni(object s = null, object e = null)
        {
            IsUpdatingRecensioni = true;
            bool res = await AdventuresPlanetManager.Instance.aggiornaRecensioni();
            if (res)
            {
                switch (SelectedMode)
                {
                    case ModalitaVisualizzazione.AlphaKey:
                        GroupByLetter();
                        break;
                    case ModalitaVisualizzazione.Voto:
                        GroupByVoto();
                        break;
                    case ModalitaVisualizzazione.Autore:
                        GroupByAuthor();
                        break;
                    default:
                        GroupByLetter();
                        break;
                }
            }
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
        private ModalitaVisualizzazione _selectedMode = ModalitaVisualizzazione.Nessuno;
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
            NavigationService.Navigate(typeof(ViewRecensione), item);
        }
        public void GoToSearch(object s, object e)
        {
            NavigationService.Navigate(typeof(SearchPage), "Recensioni");
        }
    }
    public enum ModalitaVisualizzazione
    {
        Nessuno,
        AlphaKey,
        Voto,
        Autore
    }
}
