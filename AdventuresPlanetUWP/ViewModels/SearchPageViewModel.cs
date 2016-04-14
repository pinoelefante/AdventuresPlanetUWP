using AdventuresPlanetUWP.Classes;
using AdventuresPlanetUWP.Classes.Data;
using AdventuresPlanetUWP.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace AdventuresPlanetUWP.ViewModels
{
    public class SearchPageViewModel :Mvvm.ViewModelBase
    {
        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            string kind = parameter.ToString();
            if (kind.Equals("Recensioni"))
            {
                IsRecensioni = true;
                IsSoluzioni = false;
            }
            else if(kind.Equals("Soluzioni"))
            {
                IsRecensioni = false;
                IsSoluzioni = true;
            }
            return base.OnNavigatedToAsync(parameter, mode, state);
        }
        
        private List<PaginaContenuti> listContenuti;
        public List<PaginaContenuti> ListSearch
        {
            get
            {
                return listContenuti;
            }
            set
            {
                Set<List<PaginaContenuti>>(ref listContenuti, value);
            }
        }
        private bool _rec, _sol;
        public bool IsRecensioni
        {
            get
            {
                return _rec;
            }
            set
            {
                Set<bool>(ref _rec, value);
            }
        }
        public bool IsSoluzioni
        {
            get
            {
                return _sol;
            }
            set
            {
                Set<bool>(ref _sol, value);
            }
        }
        private string lastSearch = string.Empty;
        private static readonly List<PaginaContenuti> EMPTY_LIST = new List<PaginaContenuti>(1);
        public void DoSearch(object sender, object e)
        {
            string text = string.Empty;
            if (sender is TextBox)
                text = (sender as TextBox)?.Text?.Trim();
            
            if (text.Equals(lastSearch))
                return;

            ListSearch?.Clear();
            if (string.IsNullOrEmpty(text))
            {
                ListSearch = EMPTY_LIST;
            }
            else
            {
                if (IsRecensioni)
                    ListSearch = AdventuresPlanetManager.Instance.ListaRecensioni.FindAll(x => x.Titolo.ToLower().Contains(text.ToLower()))?.Cast<PaginaContenuti>().ToList();
                else if(IsSoluzioni)
                    ListSearch = AdventuresPlanetManager.Instance.ListaSoluzioni.FindAll(x => x.Titolo.ToLower().Contains(text.ToLower()))?.Cast<PaginaContenuti>().ToList();
            }
            lastSearch = text;
        }
        public void Open(PaginaContenuti pc)
        {
            if(IsRecensioni)
                NavigationService.Navigate(typeof(ViewRecensione), pc);
            else if(IsSoluzioni)
                NavigationService.Navigate(typeof(ViewSoluzione), pc);
        }
    }
}
