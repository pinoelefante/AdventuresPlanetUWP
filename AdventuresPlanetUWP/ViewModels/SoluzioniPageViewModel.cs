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
        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if(ListaSoluzioni==null ||ListaSoluzioni.Count == 0)
                GroupByAlpha();
            return Task.CompletedTask;
        }
        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            ListaSoluzioni?.Clear();
            ListaSoluzioni = null;
            Debug.WriteLine("Soluzioni on navigatedFromAsync");
            return base.OnNavigatedFromAsync(state, suspending);
        }
        public void GroupByAlpha()
        {
            ListaSoluzioni?.Clear();
            SelectedMode = ModalitaVisualizzazione.AlphaKey;
            ListaSoluzioni = MyGrouping<SoluzioneItem>.AlphaKeyGroup(AdventuresPlanetManager.Instance.ListaSoluzioni, 
                                                                    (x) => { return x.Titolo; }, 
                                                                    true);
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
            AdventuresPlanetManager.Instance.changeIsPreferita(sol.Id, true);
        }
        public void DelPreferiti(SoluzioneItem sol)
        {
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
