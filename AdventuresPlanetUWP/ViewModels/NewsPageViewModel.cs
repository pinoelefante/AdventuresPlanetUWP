using AdventuresPlanetUWP.Classes;
using AdventuresPlanetUWP.Classes.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace AdventuresPlanetUWP.ViewModels
{
    public class NewsPageViewModel : Mvvm.ViewModelBase
    {
        public NewsPageViewModel()
        {
            
        }
        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (!AdventuresPlanetManager.Instance.IsNewsFirstLoad)
            {
                loadNews();
            }
        }
        public ObservableCollection<News> ListNews { get; } = AdventuresPlanetManager.Instance.ListaNews;
        public bool _updatingNews;
        public bool IsUpdatingNews
        {
            get
            {
                return _updatingNews;
            }
            set
            {
                Set<bool>(ref _updatingNews, value);
            }
        }
        public void openNews(News n)
        {
            if (AdventuresPlanetManager.isRecensione(n.Link))
            {
                string id = AdventuresPlanetManager.getUrlParameter(new Uri(n.Link), "game");
                RecensioneItem rec = AdventuresPlanetManager.Instance.GetRecensione(id);
                if (rec == null)
                    rec = new RecensioneItem(n.Titolo, "", "", n.Link) { isTemporary = true };
                NavigationService.Navigate(typeof(Views.ContentsPage), rec);
            }
            else if (AdventuresPlanetManager.isSoluzione(n.Link))
            {
                string id = AdventuresPlanetManager.getUrlParameter(new Uri(n.Link), "game");
                SoluzioneItem sol = AdventuresPlanetManager.Instance.GetSoluzione(id);
                if (sol == null)
                    sol = new SoluzioneItem(n.Titolo, "", n.Link) { isTemporary = true };
                NavigationService.Navigate(typeof(Views.ContentsPage), sol);
            }
            else 
                NavigationService.Navigate(typeof(Views.ViewNewsPage), n);
        }
        
        public async void loadNews(object s = null, object e = null)
        {
            IsUpdatingNews = true;
            await AdventuresPlanetManager.Instance.loadListNews();
            IsUpdatingNews = false;
        }
        public async void AggiornaNews(object sender, object e)
        {
            IsUpdatingNews = true;
            await AdventuresPlanetManager.Instance.aggiornaNews();
            IsUpdatingNews = false;
        }
        public async void showAnteprima(News n)
        {
            await new MessageDialog(n.AnteprimaNews, n.Titolo).ShowAsync();
        }
    }
}
