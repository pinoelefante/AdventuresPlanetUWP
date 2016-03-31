using AdventuresPlanetUWP.Classes;
using AdventuresPlanetUWP.Classes.Data;
using AdventuresPlanetUWP.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace AdventuresPlanetUWP.ViewModels
{
    public class ContentsPageViewModel : Mvvm.ViewModelBase
    {
        public ContentsPageViewModel()
        {
            ListaComponenti = new ObservableCollection<FrameworkElement>();
            Indice = new ObservableCollection<IndiceItem>();
            if (DesignMode.DesignModeEnabled)
            {
                ComponentiVideo = 1;
            }
        }
        private ResourceLoader res = ResourceLoader.GetForCurrentView("Contents");
        private DataTransferManager _transferManager;
        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            VideoPlayerManager.Instance.Clear();
            if (state.ContainsKey(nameof(Item)))
            {
                SetStato(state[nameof(Item)] as PaginaContenuti);
            }
            else
            {
                SetStato(parameter as PaginaContenuti);
            }
            _transferManager = DataTransferManager.GetForCurrentView();
            _transferManager.DataRequested += OnDataRequest;

           return Task.CompletedTask;
        }

        private void OnDataRequest(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.SetWebLink(new Uri(Item.Link));
            if(IsRecensione)
            {
                args.Request.Data.Properties.Title = res.GetString("contents_share_rece") + Item.Titolo;
                args.Request.Data.Properties.Description = (Item as RecensioneItem).TestoBreve;
            }
            else if (IsSoluzione)
            {
                args.Request.Data.Properties.Title = res.GetString("contents_share_rece") + Item.Titolo;
            }
        }
        public void Share(object sender, object e)
        {
            DataTransferManager.ShowShareUI();
        }
        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            Shell.SetBusy(false);
            _transferManager.DataRequested -= OnDataRequest;
            return base.OnNavigatedFromAsync(state, suspending);
        }
        private PaginaContenuti _item;
        public PaginaContenuti Item
        {
            get
            {
                return _item;
            }
            set
            {
                Set<PaginaContenuti>(ref _item, value);
            }
        }
        public RecensioneItem ItemRecensione
        {
            get
            {
                return Item as RecensioneItem;
            }
        }
        public SoluzioneItem ItemSoluzione
        {
            get
            {
                return Item as SoluzioneItem;
            }
        }
        private bool _rec, _sol;
        public bool IsRecensione
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
        public bool IsSoluzione
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
        public bool ItemLoaded { get; set; }
        private async void SetStato(PaginaContenuti cont)
        {
            Item = cont;
            IsRecensione = cont is RecensioneItem;
            IsSoluzione = cont is SoluzioneItem;
            IsVideo = Item.isVideo;
            if (ItemLoaded = await ScaricaContenuti())
            {
                AssemblaComponenti();
                LoadAlternative();
            }
            else
                Debug.WriteLine("errore durante il download");
        }

        private ObservableCollection<FrameworkElement> _components;
        public ObservableCollection<FrameworkElement> ListaComponenti
        {
            get
            {
                return _components;
            }
            private set
            {
                Set<ObservableCollection<FrameworkElement>>(ref _components, value);
            }
        }
        private bool _isVideo,_hasRec,_hasSol;
        public bool IsVideo
        {
            get
            {
                return _isVideo;
            }
            set
            {
                Set<bool>(ref _isVideo, value);
            }
        }
        public bool HasRecensione
        {
            get
            {
                return _hasRec;
            }
            set
            {
                Set<bool>(ref _hasRec, value);
            }
        }
        public bool HasSoluzione
        {
            get
            {
                return _hasSol;
            }
            set
            {
                Set<bool>(ref _hasSol, value);
            }
        }
        private int _componentiVideo, _componentiTesto, _componentiIndice;
        public int ComponentiVideo
        {
            get
            {
                return _componentiVideo;
            }
            set
            {
                Set<int>(ref _componentiVideo, value);
            }
        }
        public int ComponentiTesto
        {
            get
            {
                return _componentiTesto;
            }
            set
            {
                Set<int>(ref _componentiTesto, value);
            }
        }
        public int ComponentiIndice
        {
            get
            {
                return _componentiIndice;
            }
            set
            {
                Set<int>(ref _componentiIndice, value);
            }
        }
        private ObservableCollection<IndiceItem> _indice;
        public ObservableCollection<IndiceItem> Indice
        {
            get
            {
                return _indice;
            }
            set
            {
                Set<ObservableCollection<IndiceItem>>(ref _indice, value);
            }
        }
        private async Task<Boolean> ScaricaContenuti()
        {
            if (Item is RecensioneItem)
                DatabaseSystem.Instance.loadDettagliRecensione(Item as RecensioneItem);
            else if (Item is SoluzioneItem)
                DatabaseSystem.Instance.loadDettagliSoluzione(Item as SoluzioneItem);
            else
                return false;

            if ((string.IsNullOrEmpty(Item.Testo) && (Item.TestoRich == null || Item.TestoRich.Count == 0)) || Item.isTemporary)
            {
                if (!App.IsInternetConnected())
                    return false;
                else
                {
                    Views.Shell.SetBusy(true, res.GetString("contents_busytext"));
                    bool isOk = IsRecensione
                        ? await AdventuresPlanetManager.Instance.loadRecensione(Item as RecensioneItem)
                        : await AdventuresPlanetManager.Instance.loadSoluzione(Item as SoluzioneItem);
                    Views.Shell.SetBusy(false);
                    if (isOk)
                    {
                        if (Item.isTemporary)
                            return true;
                        else
                        {
                            if (string.IsNullOrEmpty(Item.Testo) && (Item.TestoRich == null || Item.TestoRich.Count == 0))
                                return false;
                            else
                            {
                                if (IsRecensione)
                                    DatabaseSystem.Instance.updateRecensione(Item as RecensioneItem);
                                else
                                    DatabaseSystem.Instance.updateSoluzione(Item as SoluzioneItem);
                            }
                        }

                    }
                    return isOk;
                }
            }
            Shell.SetBusy(false);
            return true;
        }
        private async void AssemblaComponenti()
        {
            ListaComponenti.Clear();
            Indice?.Clear();
            VideoPlayerManager.Instance.ListVideo.Clear();
            ComponentiTesto = 0;
            ComponentiVideo = 0;
            ComponentiIndice = 0;
            if (Item is RecensioneItem && (Item as RecensioneItem).IsRecensioneBreve)
            {
                TextBlock tb = new TextBlock();
                tb.TextWrapping = TextWrapping.Wrap;
                tb.FontSize = Settings.Instance.DimensioneFont;
                tb.Text = (Item as RecensioneItem).TestoBreve;
                ListaComponenti.Add(tb);
                ComponentiTesto++;
            }
            else
            {
                List<string> componenti = Item.TestoRich;
                Indice.Clear();
                if (componenti == null || componenti.Count == 0)
                {
                    if (!string.IsNullOrEmpty(Item.Testo?.Trim()))
                    {
                        TextBlock tb = new TextBlock();
                        tb.TextWrapping = TextWrapping.Wrap;
                        tb.FontSize = Settings.Instance.DimensioneFont;
                        tb.Text = Item.Testo;
                        ListaComponenti.Add(tb);
                        ComponentiTesto++;
                        return;
                    }
                    else
                    {
                        OpenInBrowser();
                    }
                }
                else
                {
                    foreach (string s in componenti)
                    {
                        if (s.StartsWith("@TEXT") || s.StartsWith("@DIVIDER"))
                        {
                            string testo = s.Replace("@TEXT", "").Replace("@DIVIDER", "");
                            TextBlock tb = new TextBlock();
                            tb.TextWrapping = TextWrapping.Wrap;
                            tb.FontSize = Settings.Instance.DimensioneFont;
                            tb.Text = testo;
                            ListaComponenti.Add(tb);
                            ComponentiTesto++;
                        }
                        else if (s.StartsWith("@POSINDEX"))
                        {
                            string[] split = s.Split(new char[] { ';' });
                            string index = split[1].Substring(5);
                            string titolo = "\n" + split[2].Substring(6);
                            TextBlock tb = new TextBlock();
                            tb.FontSize = Settings.Instance.DimensioneFont + 2;
                            tb.FontWeight = FontWeights.Bold;
                            tb.HorizontalAlignment = HorizontalAlignment.Center;
                            tb.TextWrapping = TextWrapping.Wrap;
                            tb.Text = titolo;
                            tb.Tag = index;
                            ListaComponenti.Add(tb);
                        }
                        else if (s.StartsWith("@INDEX"))
                        {
                            string[] split = s.Split(new char[] { ';' });
                            string index = split[1].Substring(5);
                            string titolo = split[2].Substring(6);
                            ComponentiIndice++;
                            Indice.Add(new IndiceItem(titolo, index));
                        }
                        else if (s.StartsWith("@IMG"))
                        {
                            if(Settings.Instance.IsLoadImages && App.IsInternetConnected())
                            {
                                string url = s.Replace("@IMG", "");
                                BitmapImage img_source = new BitmapImage(new Uri(url));
                                Image immagine = new Image();
                                immagine.Source = img_source;
                                immagine.MaxWidth = 640;
                                immagine.MaxHeight = 480;
                                immagine.DoubleTapped += Immagine_DoubleTapped;
                                ListaComponenti.Add(immagine);
                                
                            }
                            else //per il calcolo della posizione
                            {
                                TextBlock tb = new TextBlock();
                                ListaComponenti.Add(tb);
                            }
                            ComponentiTesto++;
                        }
                        else if (s.StartsWith("@VIDEO;"))
                        {
                            string video = s.Replace("@VIDEO;", "");
                            string[] values = video.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            string videosrc = values[0];

                            var videoId = getYouTubeId(videosrc);
                            if (videoId != null)
                            {
                                ComponentiVideo++;
                                try
                                {
                                    var uri = await YouTube.GetVideoUriAsync(videoId, Settings.Instance.YouTubeQualityMax());
                                    if (uri != null)
                                    {
                                        Debug.WriteLine("Aggiungo video");
                                        VideoPlayerManager.Instance.ListVideo.Add(uri);
                                    }
                                    else
                                    {
                                        Debug.WriteLine("Video = url non trovata");
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Debug.WriteLine(exception.Message);
                                }
                            }
                        }
                    }
                }
            }
            VideoPlayerManager.Instance.ItemLoaded = true;
        }
        private string getYouTubeId(string url)
        {
            if (url.ToLower().Contains("youtube.com/embed/"))
            {
                var videoId = url.Substring(url.LastIndexOf('/') + 1);
                return videoId;
            }

            return null;
        }
        private void Immagine_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Image immagine = sender as Image;
            string url = (immagine.Source as BitmapImage).UriSource.AbsoluteUri.ToString();
            NavigationService.Navigate(typeof(ImageViewPage), url.ToString());
            
        }
        private void LoadAlternative()
        {
            if (IsSoluzione)
            {
                HasSoluzione = true;
                HasRecensione = DatabaseSystem.Instance.selectRecensione(Item.Id) != null;
            }
            else if (IsRecensione)
            {
                HasRecensione = true;
                HasSoluzione = DatabaseSystem.Instance.selectSoluzione(Item.Id) != null;
            }
        }
        public void OpenAlternative(object s, object e)
        {
            PaginaContenuti next = null;
            Type pageTo = null;
            if (IsRecensione)
            {
                next = DatabaseSystem.Instance.selectSoluzione(Item.Id);
                pageTo = typeof(ViewSoluzione);
            }
            else if (IsSoluzione)
            {
                next = DatabaseSystem.Instance.selectRecensione(Item.Id);
                pageTo = typeof(ViewRecensione);
            }
            if(next != null && pageTo != null)
            {
                NavigationService.Navigate(pageTo, next);
            }
        }
        public void SalvaPosizione(int index)
        {
            if (Item.isVideo || Item.isTemporary)
                return;

            if (IsSoluzione && Settings.Instance.RicordaPosizioneSoluzioni)
                Settings.Instance.SaveSoluzionePosition(Item.Id, index);
            else if (IsRecensione && Settings.Instance.RicordaPosizioneRecensioni)
                Settings.Instance.SaveRecensionePosition(Item.Id, index);
        }
        public void ChangePreferiti(object s, object e)
        {
            bool pref = !Item.IsPreferita;
            Item.IsPreferita = pref;
            AdventuresPlanetManager.Instance.changeIsPreferita(Item.Id, pref);
        }
        public async void OpenStore(object s, object e)
        {
            if (!string.IsNullOrEmpty(Item.LinkStore))
            {
                await Launcher.LaunchUriAsync(new Uri(Item.LinkStore));
            }
        }
        public async void OpenInBrowser(object s = null, object e = null)
        {
            await Launcher.LaunchUriAsync(new Uri(Item.Link));
        }
        public async void VediRecensioneBreve(object s, object e)
        {
            if (IsRecensione)
            {
                RecensioneItem rec = Item as RecensioneItem;
                await new MessageDialog(rec.TestoBreve, rec.Titolo).ShowAsync();
            }
            else
            {
                Debug.WriteLine("Recensione breve? Ma è una soluzione!");
            }
        }
        public void GoToIndex(ListView list, IndiceItem indice)
        {
            foreach(FrameworkElement elem in list.Items)
            {
                if (elem.Tag?.ToString().Equals(indice.Link) == true)
                {
                    list.ScrollIntoView(elem, ScrollIntoViewAlignment.Leading);
                    return;
                }
            }
        }
    }
    public class IndiceItem
    {
        private string titolo, link;
        public IndiceItem(string titolo, string link)
        {
            Titolo = titolo;
            Link = link;
        }
        public string Titolo { get { return titolo; } set { titolo = value; } }
        public string Link { get { return link; } set { link = value; } }
    }
}
