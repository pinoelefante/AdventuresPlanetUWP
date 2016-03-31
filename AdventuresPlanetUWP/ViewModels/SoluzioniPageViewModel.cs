using AdventuresPlanetUWP.Classes;
using AdventuresPlanetUWP.Classes.Data;
using AdventuresPlanetUWP.Classes.Grouping;
using AdventuresPlanetUWP.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace AdventuresPlanetUWP.ViewModels
{
    public class SoluzioniPageViewModel : Mvvm.ViewModelBase
    {
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (!await Settings.Instance.IsSoluzioniUpdated() && !Settings.Instance.UpdateSoluzioniManualmente)
            {
                AggiornaSoluzioni();
            }
            if (ListaSoluzioni == null || ListaSoluzioni.Count == 0)
                GroupByAlpha();
            //return Task.CompletedTask;
        }
        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            Debug.WriteLine("Soluzioni on navigatedFromAsync");
            return base.OnNavigatedFromAsync(state, suspending);
        }
        public void GroupByAlpha(object s = null, object e = null)
        {
            if (SelectedMode == ModalitaVisualizzazione.AlphaKey)
                return;
            ListaSoluzioni?.Clear();
            SelectedMode = ModalitaVisualizzazione.AlphaKey;
            ListaSoluzioni = MyGrouping<SoluzioneItem>.AlphaKeyGroup(AdventuresPlanetManager.Instance.ListaSoluzioni, 
                                                                    (x) => { return x.Titolo; }, 
                                                                    true);
        }
        public void GroupByAuthor(object s = null, object e = null)
        {
            if (SelectedMode == ModalitaVisualizzazione.Autore)
                return;
            ListaSoluzioni?.Clear();
            SelectedMode = ModalitaVisualizzazione.Autore;
            ListaSoluzioni = MyGrouping<SoluzioneItem>.StringKeyGroup(AdventuresPlanetManager.Instance.ListaSoluzioni,
                                                                        (t => t.AutoreText),
                                                                        true,
                                                                        "N.D.");
        }
        public async void AggiornaSoluzioni(object s = null, object e = null)
        {
            IsUpdatingSoluzioni = true;
            bool res = await AdventuresPlanetManager.Instance.aggiornaSoluzioni();
            if (res)
            {
                switch (SelectedMode)
                {
                    case ModalitaVisualizzazione.AlphaKey:
                        GroupByAlpha();
                        break;
                    case ModalitaVisualizzazione.Autore:
                        GroupByAuthor();
                        break;
                    default:
                        GroupByAlpha();
                        break;
                }
            }
            IsUpdatingSoluzioni = false;
        }
        private Dictionary<string, List<SoluzioneItem>> _list;
        public Dictionary<string, List<SoluzioneItem>> ListaSoluzioni
        {
            get
            {
                return _list;
            }
            set
            {
                Set<Dictionary<string, List<SoluzioneItem>>>(ref _list, value);
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
        private bool _isUpdating;
        public bool IsUpdatingSoluzioni
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
        public void AddPreferiti(SoluzioneItem sol)
        {
            AdventuresPlanetManager.Instance.changeIsPreferita(sol.Id, true);
        }
        public void DelPreferiti(SoluzioneItem sol)
        {
            AdventuresPlanetManager.Instance.changeIsPreferita(sol.Id, false);
        }
        public void Open(SoluzioneItem sol)
        {
            NavigationService.Navigate(typeof(ViewSoluzione), sol);
        }
        public void GoToSearch(object s, object e)
        {
            NavigationService.Navigate(typeof(SearchPage), "Soluzioni");
        }
    }
}
