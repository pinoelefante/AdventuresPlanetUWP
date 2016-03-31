using AdventuresPlanetUWP.Classes.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using AdventuresPlanetUWP.Classes;
using Windows.UI.Xaml;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Text;
using Windows.System;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Popups;
using AdventuresPlanetUWP.Views;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;

namespace AdventuresPlanetUWP.ViewModels
{
    public class ViewNewsViewModel : Mvvm.ViewModelBase
    {
        public ViewNewsViewModel()
        {
            _componenti = new ObservableCollection<FrameworkElement>();
        }
        private ResourceLoader res = ResourceLoader.GetForCurrentView("ViewNews");
        private DataTransferManager _dataTransferManager;
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (state.ContainsKey(nameof(Item)))
            {
                Item = state[nameof(Item)] as News;
            }
            else
            {
                Item = parameter as News;
            }
            state.Clear();

            if(await scaricaNews())
            {
                componiNews();
            }
            else
            {
                componiErrore();
            }
            _dataTransferManager = DataTransferManager.GetForCurrentView();
            _dataTransferManager.DataRequested += OnDataRequested;
        }
        private void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            e.Request.Data.Properties.Title = Item.Titolo;
            e.Request.Data.Properties.Description = Item.AnteprimaNews;
            e.Request.Data.SetWebLink(new Uri(Item.Link));
        }

        private async void componiErrore()
        {
            await new MessageDialog(res.GetString("vnews_errore_download")).ShowAsync();
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            if (suspending)
                state[nameof(Item)] = Item;
            ListComponents.Clear();
            _dataTransferManager.DataRequested -= OnDataRequested;
            Shell.SetBusy(false);
            await Task.Yield();
        }
        public void Share(object s, object e)
        {
            DataTransferManager.ShowShareUI();
        }
        public News Item { get; internal set; }

        public void ShowBusy()
        {
            Shell.SetBusy(true, res.GetString("vnews_busytext"));
        }

        public void HideBusy()
        {
            Shell.SetBusy(false);
        }
        public async void OpenInBrowser(object s, object e)
        {
            await Launcher.LaunchUriAsync(new Uri(Item.Link));
        }
        private async Task<Boolean> scaricaNews()
        {
            DatabaseSystem.Instance.selectNews(Item, true);

            if (string.IsNullOrEmpty(Item.CorpoNews) && (Item.CorpoNewsRich == null || Item.CorpoNewsRich.Count == 0))
            {
                ShowBusy();
                Boolean r = await AdventuresPlanetManager.Instance.loadNews(Item);
                if (r)
                    DatabaseSystem.Instance.updateDettagliNews(Item);
                HideBusy();
                return r;
            }
            return true;
        }
        private ObservableCollection<FrameworkElement> _componenti;
        public ObservableCollection<FrameworkElement> ListComponents
        {
            get
            {
                return _componenti;
            }
            set
            {
                Set<ObservableCollection<FrameworkElement>>(ref _componenti, value);
            }
        }
        private void componiNews()
        {
            ListComponents.Clear();
            if (Item.CorpoNewsRich != null && Item.CorpoNewsRich.Count > 0)
            {
                TextBlock c_news = new TextBlock();
                c_news.TextWrapping = TextWrapping.Wrap;
                c_news.FontSize = Settings.Instance.DimensioneFont;
                foreach (string s in Item.CorpoNewsRich)
                {
                    if (s.StartsWith("@TEXT"))
                    {
                        Run tb = new Run();
                        tb.Text = s.Replace("@TEXT", "") + " ";
                        c_news.Inlines.Add(tb);
                    }
                    else if (s.StartsWith("@BOLD"))
                    {
                        Run tb = new Run();
                        tb.Text = s.Replace("@BOLD", "") + " ";
                        tb.FontWeight = FontWeights.Bold;
                        c_news.Inlines.Add(tb);
                    }
                    else if (s.StartsWith("@ITALIC"))
                    {
                        Run tb = new Run();
                        tb.Text = s.Replace("@ITALIC", "") + " ";
                        tb.FontStyle = FontStyle.Italic;
                        c_news.Inlines.Add(tb);
                    }
                    else if (s.StartsWith("@DIVIDER"))
                    {
                        Run tb = new Run();
                        tb.Text = "\n";
                        c_news.Inlines.Add(tb);
                    }
                    else if (s.StartsWith("@ANCHOR;"))
                    {

                        string t = s.Replace("@ANCHOR;", "");
                        string link = t.Substring(t.IndexOf("link=") + "link=".Length, t.IndexOf(";text=") - ";text".Length);
                        string text = t.Substring(t.IndexOf(";text=") + ";text=".Length);
                        Hyperlink tb = new Hyperlink();
                        tb.Click += async (sender, e) =>
                        {
                            if (AdventuresPlanetManager.isSoluzione(link))
                            {
                                string id = AdventuresPlanetManager.getUrlParameter(new Uri(link), "game");
                                SoluzioneItem sol = DatabaseSystem.Instance.selectSoluzione(id);
                                if (sol != null)
                                {
                                    NavigationService.Navigate(typeof(ViewSoluzione), sol);
                                }
                                else
                                {
                                    //Debug.WriteLine("sol not found - " + link);
                                    sol = new SoluzioneItem(text, "", link) {isTemporary = true };
                                    NavigationService.Navigate(typeof(ViewSoluzione), sol);
                                }
                            }
                            else if (AdventuresPlanetManager.isRecensione(link))
                            {
                                string id = AdventuresPlanetManager.getUrlParameter(new Uri(link), "game");
                                RecensioneItem rec = DatabaseSystem.Instance.selectRecensione(id);
                                if (rec != null)
                                {
                                    NavigationService.Navigate(typeof(ViewRecensione), rec);
                                }
                                else
                                {
                                    rec = new RecensioneItem(text, "", "", link) { isTemporary = true };
                                    //Debug.WriteLine("rec not found - " + link);
                                    NavigationService.Navigate(typeof(ViewRecensione), rec);
                                }

                            }
                            else
                                await Launcher.LaunchUriAsync(new Uri(link));
                        };
                        Run link_text = new Run();
                        link_text.Text = text;
                        link_text.FontWeight = FontWeights.Bold;
                        if (AdventuresPlanetManager.isSoluzione(link) || AdventuresPlanetManager.isRecensione(link))
                        {
                            link_text.FontSize = Settings.Instance.DimensioneFont + 2;
                            link_text.Foreground = new SolidColorBrush(Colors.Orange);
                        }
                        else
                        {
                            link_text.Foreground = new SolidColorBrush(Colors.ForestGreen);
                        }

                        tb.Inlines.Add(link_text);

                        Run space = new Run();
                        space.Text = " ";

                        c_news.Inlines.Add(tb);
                        c_news.Inlines.Add(space);
                    }
                }
                ListComponents.Add(c_news);
            }
        }
    }
}
