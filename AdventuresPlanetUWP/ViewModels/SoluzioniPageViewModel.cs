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
        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            GroupByAlpha();
        }
        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            Debug.WriteLine("Soluzioni on navigatedFromAsync");
            ListaSoluzioni?.Clear();
            ListaSoluzioni = null;
            return base.OnNavigatedFromAsync(state, suspending);
        }
        public void GroupByAlpha()
        {
            ListaSoluzioni?.Clear();
            SelectedMode = ModalitaVisualizzazione.AlphaKey;
            List<SoluzioneItem> list = AdventuresPlanetManager.Instance.ListaSoluzioni;
            ListaSoluzioni = MyGrouping<SoluzioneItem>.AlphaKeyGroup(list, (x) => { return x.Titolo; }, true);
        }
        public async void AggiornaSoluzioni(object s, object e)
        {
            IsUpdatingSoluzioni = true;
            bool res = await AdventuresPlanetManager.Instance.aggiornaSoluzioni();
            if (res)
                GroupByAlpha();
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
            sol.IsPreferita = true;
            AdventuresPlanetManager.Instance.changeIsPreferita(sol.Id, true);
        }
        public void DelPreferiti(SoluzioneItem sol)
        {
            sol.IsPreferita = false;
            AdventuresPlanetManager.Instance.changeIsPreferita(sol.Id, false);
        }
        public void Open(SoluzioneItem sol)
        {
            NavigationService.Navigate(typeof(ContentsPage), sol);
        }
        public void GoToSearch(object s, object e)
        {
            NavigationService.Navigate(typeof(SearchPage), "Soluzioni");
        }
    }
}
