using AdventuresPlanetUWP.Classes;
using AdventuresPlanetUWP.Classes.Data;
using AdventuresPlanetUWP.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Store;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace AdventuresPlanetUWP.ViewModels
{
    public class NewsPageViewModel : Mvvm.ViewModelBase
    {
        private static ResourceLoader resApp = ResourceLoader.GetForCurrentView("App");
        public static NewsPageViewModel Instance { get; set; }
        private static bool started = false;
        public NewsPageViewModel()
        {
            Instance = this;
        }
        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (!started)
            {
                WarningNonItaliano();
                mostraVotaApplicazione();
                started = true;
            }
            return Task.CompletedTask;
        }
        public AdventuresPlanetManager.NewsCollection ListNews { get; } = AdventuresPlanetManager.Instance.ListaNews;
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
        private async void WarningNonItaliano()
        {
            CultureInfo info = CultureInfo.CurrentUICulture;
            Debug.WriteLine("Current lang = " + info.TwoLetterISOLanguageName);
            if (info.TwoLetterISOLanguageName != "it")
            {
                string t = resApp.GetString("avviso_soloitaliano_titolo");
                string m = resApp.GetString("avviso_soloitaliano");
                await new MessageDialog(m, t).ShowAsync();
            }
        }
        private async void mostraVotaApplicazione()
        {
            Debug.WriteLine("Numero avvio = " + Settings.Instance.NumeroAvvii);
            if (Settings.Instance.NumeroAvvii % 3 == 0 && !Settings.Instance.Votato)
            {
                string titolo = resApp.GetString("app_vota_titolo");
                string messaggio = resApp.GetString("app_vota");
                MessageDialog msg = new MessageDialog(messaggio, titolo);
                UICommand si = new UICommand(resApp.GetString("app_vota_comando_vota")) { Id = 0 };
                si.Invoked = async (x) =>
                {
                    var uri = new Uri(string.Format("ms-windows-store:reviewapp?appid={0}", CurrentApp.AppId));
                    await Launcher.LaunchUriAsync(uri);
                    Settings.Instance.Votato = true;
                };
                UICommand no = new UICommand(resApp.GetString("app_vota_comando_annulla")) { Id = 1 };
                no.Invoked = (x) => { };
                msg.Commands.Add(si);
                msg.Commands.Add(no);
                msg.CancelCommandIndex = 1;
                msg.DefaultCommandIndex = 0;

                await msg.ShowAsync();
            }
        }
    }
}
