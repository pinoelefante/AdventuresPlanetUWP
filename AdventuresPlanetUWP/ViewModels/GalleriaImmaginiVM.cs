using AdventuresPlanetUWP.Classes;
using AdventuresPlanetUWP.Classes.Data;
using AdventuresPlanetUWP.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;
using static AdventuresPlanetUWP.Classes.AdventuresPlanetManager;

namespace AdventuresPlanetUWP.ViewModels
{
    public class GalleriaImmaginiVM : Mvvm.ViewModelBase
    {
        public static GalleriaImmaginiVM Instance { get; set; }
        public GalleriaImmaginiVM()
        {
            Instance = this;
        }
        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            Dictionary<string, object> parametri = parameter as Dictionary<string, object>;
            if(mode == NavigationMode.New || mode == NavigationMode.Refresh)
            {
                if (parametri.ContainsKey("url"))
                    idGalleria = AdventuresPlanetManager.getUrlParameter(new Uri(parametri["url"].ToString()), "game");
                else if (parametri.ContainsKey("id"))
                    idGalleria = parametri["id"].ToString();

                if (parametri.ContainsKey("titolo"))
                    Titolo = parametri["titolo"].ToString();

                if (string.IsNullOrEmpty(Titolo))
                    Titolo = GetTitoloById(idGalleria);

                LinkImmagini = new ImagesCollection(idGalleria);
                WindowWrapper.Current().Dispatcher.Dispatch(() =>
                {
                    RaisePropertyChanged(nameof(LinkImmagini));
                });
            }
            return base.OnNavigatedToAsync(parameter, mode, state);
        }
        private string GetTitoloById(string id)
        {
            var res = DatabaseSystem.Instance.selectTitoloGalleria(id);
            if (string.IsNullOrEmpty(res))
            {
                RecensioneItem recensione = DatabaseSystem.Instance.selectRecensione(id);
                if (recensione != null)
                    res = recensione.Titolo;
                if (string.IsNullOrEmpty(res))
                {
                    SoluzioneItem soluzione = DatabaseSystem.Instance.selectSoluzione(id);
                    if (soluzione != null)
                        res = soluzione.Titolo;
                }
            }
            return res;
        }
        public ImagesCollection LinkImmagini { get; set; }
        public string titolo = string.Empty, idGalleria = string.Empty;
        private bool _loading = false;
        public bool IsLoading
        {
            get
            {
                return _loading;
            }
            set
            {
                Set(ref _loading, value);
            }
        }
        public void OpenImage(object s, ItemClickEventArgs e)
        {
            AdvImage url = e.ClickedItem as AdvImage;
            Debug.WriteLine("Clicked: " + url.ImageLink);
            int index = LinkImmagini.IndexOf(url);
            Dictionary<string, object> p = new Dictionary<string, object>();
            p["index"] = index;
            p["immagini"] = LinkImmagini.ToListUrls();
            p["titolo"] = titolo;
            NavigationService.Navigate(typeof(ImageViewPage), p);
        }
        public string Titolo
        {
            get
            {
                return titolo;
            }
            set
            {
                Set(ref titolo, value);
            }
        }
        
        public class ImagesCollection : ObservableCollection<AdvImage>, ISupportIncrementalLoading, INotifyPropertyChanged
        {
            private int pagina = 1;
            public ImagesCollection(string url)
            {
                MainUrl = url;
                Reset();
            }
            public string MainUrl { get; private set; }
            private bool _more = true;
            public bool HasMoreItems
            {
                get
                {
                    return _more;
                }
                set
                {
                    _more = value;
                }
            }

            public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
            {
                return Task.Run<LoadMoreItemsResult>(async () =>
                {
                    WindowWrapper.Current().Dispatcher.Dispatch(() =>
                    {
                        GalleriaImmaginiVM.Instance.IsLoading = true;
                    });
                    
                    Dictionary<string, object> results = await AdventuresPlanetManager.Instance.loadGalleria(MainUrl, pagina);
                    if (results != null)
                    {
                        HasMoreItems = (bool)results["HasNext"];
                        List<KeyValuePair<string, List<AdvImage>>> res = results["Images"] as List<KeyValuePair<string, List<AdvImage>>>;
                        foreach (KeyValuePair<string, List<AdvImage>> kv in res)
                        {
                            Debug.WriteLine($"gruppo {kv.Key} - {kv.Value.Count} immagini");
                            WindowWrapper.Current().Dispatcher.Dispatch(() =>
                            {
                                foreach (var img in kv.Value)
                                    Add(img);
                            });
                        }
                        pagina++;
                    }
                    else
                    {
                        //TODO manage error
                    }
                    WindowWrapper.Current().Dispatcher.Dispatch(() =>
                    {
                        GalleriaImmaginiVM.Instance.IsLoading = false;
                    });
                    return new LoadMoreItemsResult() { Count = count };

                }).AsAsyncOperation<LoadMoreItemsResult>();
            }

            public void Reset()
            {
                Clear();
                pagina = 1;
            }
            public List<string> ToListUrls()
            {
                List<string> list = new List<string>(this.Count);
                foreach(var item in this)
                    list.Add(item.ImageLink);
                return list;
            }
        }
    }
}
