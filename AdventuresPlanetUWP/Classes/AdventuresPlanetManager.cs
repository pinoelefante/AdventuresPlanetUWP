using AdventuresPlanetUWP.Classes.Data;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.Web.Http.Filters;
using static System.Diagnostics.Debug;
using Windows.Foundation;
using AdventuresPlanetUWP.ViewModels;
using Template10.Common;

namespace AdventuresPlanetUWP.Classes
{
    public class AdventuresPlanetManager
    {
        private static AdventuresPlanetManager singleton;
        private HttpClient http;
        private Windows.Web.Http.HttpClient jsonClient;
        //public static string URL_PODCAST = "http://www.adventuresplanet.it/podcast.php";
        public static string URL_BASE = "http://www.adventuresplanet.it/";
        //public static string URL_RECENSIONI = "http://www.adventuresplanet.it/recensioni.php";
        //public static string URL_SOLUZIONI = "http://www.adventuresplanet.it/soluzioni.php";
        //public static string URL_NEWS_MESI = "http://www.adventuresplanet.it/index.php?old=si";

        public List<RecensioneItem> ListaRecensioni { get; set; }
        public List<SoluzioneItem> ListaSoluzioni { get; set; }
        public ObservableCollection<PodcastItem> ListaPodcast { get; set; }
        public NewsCollection ListaNews { get; } = new NewsCollection();
        public bool IsNewsFirstLoad { get; set; }
        public bool IsPodcastFirstLoad { get; set; }
        private AdventuresPlanetManager()
        {
            if (http == null)
                http = new HttpClient();

            if (jsonClient == null)
            {
                HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
                filter.AutomaticDecompression = true;
                jsonClient = new Windows.Web.Http.HttpClient(filter);
                jsonClient.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
            }
        }
        public void Load()
        {
            ListaPodcast = DatabaseSystem.Instance.selectAllPodcastOb();
            ListaRecensioni = DatabaseSystem.Instance.selectAllRecensioniLite();
            ListaSoluzioni = DatabaseSystem.Instance.selectAllSoluzioniLite();
        }
        public static AdventuresPlanetManager Instance
        {
            get
            {
                if (singleton == null)
                    singleton = new AdventuresPlanetManager();
                return singleton;
            }
        }
        public void Reset()
        {
            ListaNews?.Reset();
            ListaRecensioni?.Clear();
            ListaSoluzioni?.Clear();
            ListaPodcast?.Clear();
        }
        private async Task<List<PodcastItem>> getAggiornamentiPodcast(string response = null)
        {

            long last_update = Settings.Instance.LastPodcastUpdate;
            if (response == null)
                response = await jsonClient.GetStringAsync(new Uri("http://pinoelefante.altervista.org/avp_it/avp_podcast.php?from=" + last_update));
            JsonObject resp_json = JsonObject.Parse(response);
            long time = (long)resp_json["time"].GetNumber();
            JsonArray list_pj = resp_json["podcast"].GetArray();
            List<PodcastItem> list_podcast = new List<PodcastItem>(list_pj.Count);
            for (int i = 0; i < list_pj.Count; i++)
            {
                JsonObject pod = list_pj[i].GetObject();
                string titolo = pod["titolo"].GetString();
                string descr = pod["descrizione"].GetString();
                string link = pod["link"].GetString();
                string pubD = pod["pubData"].GetString();
                PodcastItem item = new PodcastItem(titolo, pubD, link);
                item.Descrizione = descr;
                list_podcast.Add(item);
            }
            Settings.Instance.LastPodcastUpdate = time;
            return list_podcast;
        }
        public async Task<bool> loadPodcast()
        {
            App.KeepScreenOn();
            List<PodcastItem> list = await getAggiornamentiPodcast();
            int count = list.Count;
            if (count > 0)
            {
                DatabaseSystem.Instance.insertPodcast(list);
            }
            list.Clear();
            App.KeepScreenOn_Release();
            return count > 0;
        }
        private async Task<List<RecensioneItem>> getAggiornamentiRecensioni(string response = null)
        {
            WriteLine("inizio = " + DateTime.Now.ToLocalTime());
            long last_update = Settings.Instance.LastRecensioniUpdate;
            if (response == null)
                response = await jsonClient.GetStringAsync(new Uri("http://pinoelefante.altervista.org/avp_it/avp_rece.php?from=" + last_update));
            JsonObject resp_json = JsonObject.Parse(response);
            long time = (long)resp_json["time"].GetNumber(); ;
            JsonArray list_avv = resp_json["avventure"].GetArray();
            Debug.WriteLine("Time = " + time);
            List<RecensioneItem> list_recensioni = new List<RecensioneItem>(list_avv.Count);
            for (int i = 0; i < list_avv.Count; i++)
            {
                JsonObject avv = list_avv[i].GetObject();
                string nome = avv["titolo"].GetString().Trim();
                string link = URL_BASE + avv["link"].GetString().Trim();
                string autore = avv["autore"].GetString().Trim();
                string voto = avv["voto"].GetString().Trim();
                RecensioneItem item = new RecensioneItem(nome, autore, voto, link);
                list_recensioni.Add(item);
            }
            Settings.Instance.LastRecensioniUpdate = time;
            WriteLine("fine = " + DateTime.Now.ToLocalTime());
            return list_recensioni;
        }
        private async Task<List<SoluzioneItem>> getAggiornamentiSoluzioni(string response = null)
        {
            WriteLine("inizio = " + DateTime.Now.ToLocalTime());
            long last_update = Settings.Instance.LastSoluzioniUpdate;
            if (response == null)
                response = await jsonClient.GetStringAsync(new Uri("http://pinoelefante.altervista.org/avp_it/avp_solu.php?from=" + last_update));
            JsonObject resp_json = JsonObject.Parse(response);
            long time = (long)resp_json["time"].GetNumber(); ;
            JsonArray list_avv = resp_json["avventure"].GetArray();
            Debug.WriteLine("Time = " + time);
            List<SoluzioneItem> list_soluzioni = new List<SoluzioneItem>(list_avv.Count);
            for (int i = 0; i < list_avv.Count; i++)
            {
                JsonObject avv = list_avv[i].GetObject();
                string nome = avv["titolo"].GetString().Trim();
                string link = URL_BASE + avv["link"].GetString().Trim();
                string autore = avv["autore"].GetString().Trim();
                SoluzioneItem item = new SoluzioneItem(nome, autore, link);
                list_soluzioni.Add(item);
            }
            Settings.Instance.LastSoluzioniUpdate = time;
            WriteLine("fine = " + DateTime.Now.ToLocalTime());
            return list_soluzioni;
        }
        public async Task initPodcastFromJsonFile()
        {
            App.KeepScreenOn();
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///podcast.json"));
            string fileContent;
            using (StreamReader sRead = new StreamReader(await file.OpenStreamForReadAsync()))
            {
                fileContent = await sRead.ReadToEndAsync();
            }
            List<PodcastItem> list = await getAggiornamentiPodcast(fileContent);
            if (list.Count > 0)
                DatabaseSystem.Instance.insertPodcast(list);
            App.KeepScreenOn_Release();
        }
        public async Task initRecensioniFromJsonFile()
        {
            App.KeepScreenOn();
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///recensioni.json"));
            string fileContent;
            using (StreamReader sRead = new StreamReader(await file.OpenStreamForReadAsync()))
            {
                fileContent = await sRead.ReadToEndAsync();
            }
            List<RecensioneItem> list = await getAggiornamentiRecensioni(fileContent);
            if (list.Count > 0)
                DatabaseSystem.Instance.insertRecensione(list);
            App.KeepScreenOn_Release();
        }
        public async Task initSoluzioniFromJsonFile()
        {
            App.KeepScreenOn();
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///soluzioni.json"));
            string fileContent;
            using (StreamReader sRead = new StreamReader(await file.OpenStreamForReadAsync()))
            {
                fileContent = await sRead.ReadToEndAsync();
            }
            List<SoluzioneItem> list = await getAggiornamentiSoluzioni(fileContent);
            if (list.Count > 0)
                DatabaseSystem.Instance.insertSoluzione(list);
            App.KeepScreenOn_Release();
        }
        public async Task<Boolean> aggiornaRecensioni()
        {
            App.KeepScreenOn();
            List<RecensioneItem> list = await getAggiornamentiRecensioni();
            int count = list.Count;
            if (list.Count > 0)
            {
                DatabaseSystem.Instance.insertRecensione(list, true);
                ListaRecensioni.AddRange(list);
            }
            list.Clear();
            App.KeepScreenOn_Release();
            return count > 0;
        }
        public async Task aggiornaPodcast()
        {
            App.KeepScreenOn();
            List<PodcastItem> newList = await getAggiornamentiPodcast();
            for (int i = newList.Count - 1; i >= 0; i--)
            {
                PodcastItem pod = newList[i];
                DatabaseSystem.Instance.insertPodcast(pod);
                ListaPodcast.Insert(0, pod);
            }
            App.KeepScreenOn_Release();
        }

        private readonly static List<News> EMPTY_LIST = new List<News>();
        public async Task<List<News>> loadListNews(int anno, int mese)
        {
            Debug.WriteLine($"loadListNews {anno}{mese}");
            //Debug.WriteLine("NewsLastUpdate = {0}\nIsNewsUpdated = {1}", Settings.Instance.LastNewsUpdate, Settings.Instance.IsNewsUpdated);
            List<News> list_news = EMPTY_LIST;

            string meselink = GetPeriodoString(anno, mese);
            try
            {
                if (Settings.Instance.isNewsMesePersistent(meselink))
                {
                    list_news = DatabaseSystem.Instance.selectNewsByMeseLink(meselink);
                    if (list_news?.Count == 0)
                    {
                        if (App.IsInternetConnected())
                        {
                            list_news = await parsePageNews(anno,mese);
                            DatabaseSystem.Instance.deleteNewsByMeseLink(meselink);
                            DatabaseSystem.Instance.insertNews(list_news);
                            return DatabaseSystem.Instance.selectNewsByMeseLink(meselink);
                        }
                        else
                        {
                            Settings.Instance.setNewsMesePersistent(meselink, false);
                            return null;
                        }
                    }
                    else
                    {
                        return list_news;
                    }
                }
                else //mese non persistente
                {
                    string meselinkCurrPer = GetMeseCorrenteString();
                    if (App.IsInternetConnected())
                    {
                        list_news = await parsePageNews(anno,mese);
                        if(list_news?.Count == 0)
                        {
                            return null;
                        }
                        else
                        {
                            if(meselink.CompareTo(meselinkCurrPer) == 0) //mese corrente da non rendere persistente
                            {
                                DatabaseSystem.Instance.deleteNewsByMeseLink(meselink);
                                DatabaseSystem.Instance.insertNews(list_news);
                                return DatabaseSystem.Instance.selectNewsByMeseLink(meselink);
                            }
                            else
                            {
                                DatabaseSystem.Instance.deleteNewsByMeseLink(meselink);
                                DatabaseSystem.Instance.insertNews(list_news);
                                Settings.Instance.setNewsMesePersistent(meselink, true);
                                return DatabaseSystem.Instance.selectNewsByMeseLink(meselink);
                            }
                        }
                    }
                    else
                    {
                        list_news = DatabaseSystem.Instance.selectNewsByMeseLink(meselink);
                        if (list_news == null || list_news.Count == 0)
                            return null;
                        else
                            return list_news;
                    }
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine("Eccezione loadListNews " + e.Message);
                return null;
            }
        }
        private async Task<List<News>> parsePageNews(int anno, int mese)
        {
            string meselink = GetPeriodoString(anno, mese);
            string meseTimestamp = Settings.getUnixTimeStamp(anno, mese).ToString();
            string page = $"{URL_BASE}/index.php?old=si&data={meseTimestamp}";
            string response = await http.GetStringAsync(new Uri(page));
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);
            IEnumerable<HtmlNode> news = doc.DocumentNode.Descendants("div").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value.Equals("main_news"));
            Debug.WriteLine("news caricate: " + news.Count());
            List<News> list = new List<News>(news.Count());
            foreach (HtmlNode node in news)
            {
                string data = node.Descendants("h1").ToArray()[0].InnerText.Trim();
                HtmlNode n_titolo = node.Descendants("a").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value.Equals("news_title")).ToArray()[0];
                string link = URL_BASE + n_titolo.Attributes["href"].Value;
                string titolo = WebUtility.HtmlDecode(n_titolo.InnerText.Trim());
                string img = URL_BASE + node.Descendants("span").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value.Equals("padd")).ToArray()[0].Descendants("img").ToArray()[0].Attributes["src"].Value;
                HtmlNode news_ante_cont = node.Descendants("p").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value.Equals("news")).ToArray()[0];
                string news_s = WebUtility.HtmlDecode(news_ante_cont.InnerText.Trim());

                if (link.CompareTo(URL_BASE + "index.php") == 0 || news_ante_cont.ChildNodes.Count == 1)
                {
                    HtmlNode anchor = news_ante_cont.Descendants("a").Count() == 1 ? news_ante_cont.FirstChild : null;
                    if (anchor != null)
                    {
                        string tlink = URL_BASE + anchor.Attributes["href"].Value;
                        if (isRecensione(tlink) || isSoluzione(tlink))
                            link = tlink;
                    }
                }

                News news_item = new News();
                news_item.AnteprimaNews = news_s;
                news_item.DataPubblicazione = data;
                news_item.Immagine = img;
                news_item.Link = link;
                news_item.Titolo = titolo;
                news_item.MeseLink = meselink;

                list.Add(news_item);
            }
            return list;
        }
        public async Task aggiornaNews()
        {
            /*
            App.KeepScreenOn();
            List<News> news = new List<News>();
            if (App.IsInternetConnected())
            {
                int oldMesiSize = mesi_news.Count;
                if (await loadMesiNewsOnline() && mesi_news?.Count > 0)
                {
                    int newMesiSize = mesi_news.Count;
                    int mesiInseriti = newMesiSize - oldMesiSize;
                    current_mese = current_mese + mesiInseriti;
                    for (int i = 0; i < current_mese; i++) //scarica news dei nuovi mesi inseriti
                    {
                        if (Settings.Instance.isNewsMesePersistent(mesi_news[i].Key))
                            break;
                        List<News> news_nuove = await parsePageNews(mesi_news[i].Key);
                        news_nuove = DatabaseSystem.Instance.insertNews(news_nuove);
                        news.AddRange(news_nuove);

                        if (i > 0)
                            Settings.Instance.setNewsMesePersistent(mesi_news[i].Key, true);
                    }
                    Settings.Instance.LastNewsUpdate = Settings.getUnixTimeStamp();
                }
            }
            for (int i = news.Count - 1; i >= 0; i--)
            {
                ListaNews.Insert(0, news[i]);
            }
            App.KeepScreenOn_Release();
            */
        }

        public async Task<Boolean> loadNews(News n)
        {
            App.KeepScreenOn();
            try
            {
                string response = await http.GetStringAsync(new Uri(n.Link));
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(response);

                HtmlNode news = doc.DocumentNode.Descendants("p").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value.Equals("news")).ToArray()[0];
                n.CorpoNews = WebUtility.HtmlDecode(news.InnerText.Trim());
                n.CorpoNewsRich = parseNewsRich(news);
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
            finally
            {
                App.KeepScreenOn_Release();
            }
        }

        public async Task<Boolean> loadRecensione(RecensioneItem rec)
        {
            App.KeepScreenOn();
            try
            {
                Debug.WriteLine("Scarico recensione: " + rec.Link);
                string response = await http.GetStringAsync(rec.Link);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(response);

                HtmlNode rec_s = doc.GetElementbyId("scheda_breve");
                if (rec_s != null)
                    rec.TestoBreve = WebUtility.HtmlDecode(rec_s.InnerText.Trim());

                HtmlNode vr = doc.GetElementbyId("bar_vote");
                if (vr != null)
                    rec.VotoText = string.IsNullOrEmpty(rec.VotoText) ? vr.InnerText.Trim() : rec.VotoText;

                HtmlNode vu = doc.GetElementbyId("bar_vote2");
                if (vu != null)
                    rec.VotoUtentiText = vu.InnerText.Trim();

                HtmlNode rec_l = doc.GetElementbyId("scheda_completa");
                if (rec_l != null)
                {
                    Debug.WriteLine(rec_l.InnerHtml);
                    rec.Testo = WebUtility.HtmlDecode(rec_l.InnerText.Trim());
                    rec.TestoRich = parseTestoRich(rec_l);
                }

                HtmlNode shop = doc.GetElementbyId("sch_shop_box");
                if (shop != null)
                {
                    IEnumerable<HtmlNode> link = shop.Descendants("a").Where(x => x.Attributes.Contains("href"));
                    if (link.Count() > 0)
                    {
                        rec.LinkStore = link.ElementAt(0).Attributes["href"].Value;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
            finally
            {
                App.KeepScreenOn_Release();
            }
        }
        private List<String> parseNewsRich(HtmlNode node)
        {
            List<string> rich = new List<string>();

            foreach (HtmlNode n in node.ChildNodes)
            {
                if (n.GetType() == typeof(HtmlTextNode))
                    rich.Add("@TEXT" + WebUtility.HtmlDecode(n.InnerText.Trim()));
                else
                {
                    if (n.OriginalName.Equals("a"))
                    {
                        string link = WebUtility.HtmlDecode(n.Attributes["href"].Value);
                        string text = WebUtility.HtmlDecode(n.InnerText.Trim());
                        rich.Add("@ANCHOR;link=" + link + ";text=" + text);
                    }
                    else if (n.OriginalName.Equals("b") || n.OriginalName.Equals("strong"))
                    {
                        if (n.FirstChild.GetType() == typeof(HtmlTextNode))
                            rich.Add("@BOLD" + WebUtility.HtmlDecode(n.FirstChild.InnerText));
                        else
                        {
                            if (n.FirstChild.OriginalName.Equals("a"))
                            {
                                string link = WebUtility.HtmlDecode(n.FirstChild.Attributes["href"].Value);
                                string text = WebUtility.HtmlDecode(n.FirstChild.InnerText.Trim());
                                rich.Add("@ANCHOR;link=" + link + ";text=" + text);
                            }
                        }
                    }
                    else if (n.OriginalName.Equals("i"))
                    {
                        string text = WebUtility.HtmlDecode(n.FirstChild.InnerText.Trim());
                        rich.Add("@ITALIC" + text);
                    }
                    else if (n.OriginalName.Equals("br"))
                    {
                        rich.Add("@DIVIDER");
                    }
                }
            }

            return rich;
        }
        private List<string> parseTestoRich(HtmlNode scheda)
        {
            List<string> rich = new List<string>();
            foreach (HtmlNode n in scheda.ChildNodes)
            {
                if (n.GetType() == typeof(HtmlTextNode))
                {
                    string newstring = WebUtility.HtmlDecode(n.InnerText.Trim());
                    if (newstring.Length > 0)
                        rich.Add("@TEXT" + newstring);
                }
                else
                {
                    if (n.Attributes.Contains("class") && n.Attributes["class"].Value.Contains("scheda_immagini"))
                    {
                        string img = n.Descendants("a").ToArray()[0].Attributes["href"].Value;
                        rich.Add("@IMG" + img);
                    }
                    else if (n.OriginalName.CompareTo("br") == 0)
                    {
                        if ((rich.Count - 1 >= 0) && !rich.ElementAt(rich.Count - 1).StartsWith("@DIVIDER"))
                            rich.Add("@DIVIDER");
                    }
                    else if (n.OriginalName.Equals("h3")) //indice soluzione
                    {
                        IEnumerable<HtmlNode> indice = n.Descendants("a");
                        foreach (HtmlNode i in indice)
                        {
                            string titolo = WebUtility.HtmlDecode(i.InnerText.Trim());
                            string index = WebUtility.HtmlDecode(i.Attributes["href"].Value.Trim());
                            if (titolo.Length > 0 && index.Length > 0)
                                rich.Add("@INDEX;link=" + index + ";title=" + titolo);
                        }
                    }
                    else if (n.OriginalName.Equals("table"))
                    {
                        IEnumerable<HtmlNode> indiceItem = n.Descendants("a").Where(x => x.Attributes.Contains("name"));
                        foreach (HtmlNode i in indiceItem)
                        {
                            string index = "#" + WebUtility.HtmlDecode(i.Attributes["name"].Value.Trim());
                            string titolo = WebUtility.HtmlDecode(i.InnerText.Trim());
                            rich.Add("@POSINDEX;link=" + index + ";title=" + titolo);
                        }
                    }
                    else if (n.OriginalName.Equals("iframe")) //per videosoluzione
                    {
                        Debug.WriteLine("trovato video");
                        if (n.Attributes.Contains("src"))
                        {
                            string src = n.Attributes["src"].Value;
                            if (!src.StartsWith("http"))
                            {
                                src = "http:" + src;
                            }
                            string height = n.Attributes["height"].Value;
                            string width = n.Attributes["width"].Value;
                            Debug.WriteLine(src);
                            rich.Add("@VIDEO;" + src + ";" + width + ";" + height);
                        }
                    }
                    else
                    {
                        Debug.WriteLine(n.OriginalName);
                        string newstring = WebUtility.HtmlDecode(n.InnerText.Trim());
                        if (newstring.Length == 0)
                            continue;

                        if ((rich.Count - 1) >= 0 && rich.ElementAt(rich.Count - 1).StartsWith("@TEXT"))
                        {
                            string old = rich.ElementAt(rich.Count - 1);
                            old += " " + newstring;
                            rich.RemoveAt(rich.Count - 1);
                            rich.Add(old);
                        }
                        else
                            rich.Add("@TEXT" + newstring);
                    }
                }
            }
            compattaRichText(rich);
            return rich;
        }
        private void compattaRichText(List<string> rich)
        {
            for (int i = 0; i < rich.Count - 1;)
            {
                if (rich[i].StartsWith("@TEXT") && rich[i + 1].StartsWith("@TEXT"))
                {
                    rich[i] += " " + rich[i + 1].Replace("@TEXT", "");
                    rich.RemoveAt(i + 1);
                }
                else
                    i++;
            }
        }
        public async Task<Boolean> aggiornaSoluzioni()
        {
            App.KeepScreenOn();
            List<SoluzioneItem> list_sol = await getAggiornamentiSoluzioni();
            int count = list_sol.Count;
            if (count > 0)
            {
                DatabaseSystem.Instance.insertSoluzione(list_sol, true);
                ListaSoluzioni.AddRange(list_sol);
            }
            list_sol.Clear();
            App.KeepScreenOn_Release();
            return count > 0;
        }
        public async Task<Boolean> loadSoluzione(SoluzioneItem sol)
        {
            App.KeepScreenOn();
            try
            {
                string response = await http.GetStringAsync(sol.Link);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(response);

                HtmlNode rec_l = doc.GetElementbyId("scheda_completa");
                if (rec_l != null)
                {
                    sol.Testo = WebUtility.HtmlDecode(rec_l.InnerText.Trim());
                    sol.TestoRich = parseTestoRich(rec_l);
                }
                HtmlNode shop = doc.GetElementbyId("sch_shop_box");
                if (shop != null)
                {
                    IEnumerable<HtmlNode> link = shop.Descendants("a").Where(x => x.Attributes.Contains("href"));
                    if (link.Count() > 0)
                    {
                        sol.LinkStore = link.ElementAt(0).Attributes["href"].Value;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
            finally
            {
                App.KeepScreenOn_Release();
            }
        }
        public static bool isRecensione(String uri)
        {
            if (uri.StartsWith(URL_BASE + "scheda_recensione.php"))
                return true;
            return false;
        }
        public static bool isSoluzione(String uri)
        {
            if (uri.StartsWith(URL_BASE + "scheda_soluzione.php"))
                return true;
            return false;
        }
        public static bool isExtra(string uri)
        {
            if (uri.StartsWith(URL_BASE + "scheda_extra.php"))
                return true;
            return false;
        }
        public static string getUrlParameter(Uri Link, string parameter)
        {
            string[] parametri = Link.Query.Replace("&amp;", "&").Substring(1).Split(new char[] { '&' });
            for (int i = 0; i < parametri.Length; i++)
            {
                string[] kv = parametri[i].Split(new char[] { '=' });
                if (kv[0].ToLower().CompareTo(parameter) == 0)
                    return kv[1];
            }
            return null;
        }
        public void changeIsPreferita(string id, bool val)
        {
            IEnumerable<RecensioneItem> r_rec = ListaRecensioni?.Where(x => x.Id.Equals(id));
            if (r_rec?.Count() == 1)
                r_rec.ElementAt(0).IsPreferita = val;

            IEnumerable<SoluzioneItem> r_sol = ListaSoluzioni?.Where(x => x.Id.Equals(id));
            if (r_sol?.Count() == 1)
                r_sol.ElementAt(0).IsPreferita = val;
        }
        public SoluzioneItem GetSoluzione(string id)
        {
            SoluzioneItem sol = ListaSoluzioni.Find(x => x.Id.Equals(id)); ;
            if (sol == null)
                sol = DatabaseSystem.Instance.selectSoluzione(id);
            return sol;
        }
        public RecensioneItem GetRecensione(string id)
        {
            RecensioneItem rec = ListaRecensioni.Find(x => x.Id.Equals(id)); ;
            if (rec == null)
                rec = DatabaseSystem.Instance.selectRecensione(id);
            return rec;
        }
        public string GetMeseCorrenteString()
        {
            DateTime now = DateTime.Now;
            return GetPeriodoString(now.Year, now.Month);
        }
        public string GetPeriodoString(int anno, int mese)
        {
            return $"{anno.ToString("D4")}{mese.ToString("D2")}";
        }
        public class NewsCollection : ObservableCollection<News>, ISupportIncrementalLoading
        {
            private int currAnno, currMese;
            public NewsCollection()
            {
                Reset();
            }
            public bool HasMoreItems
            {
                get
                {
                    if(currAnno<=2004 && currMese < 2)
                        return false;
                    return true;
                }
            }

            public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
            {
                return Task.Run<LoadMoreItemsResult>(async () =>
                {
                    WindowWrapper.Current().Dispatcher.Dispatch(()=>
                    {
                        NewsPageViewModel.Instance.IsUpdatingNews = true;
                    });
                    
                    //1 - download o caricamento da database
                    List<News> list_news = await Instance.loadListNews(currAnno, currMese);
                    //2 - aggiunta alla collezione
                    if(list_news != null && list_news.Count > 0)
                    {
                        WindowWrapper.Current().Dispatcher.Dispatch(() =>
                        {
                            foreach (News n in list_news)
                            {
                                Add(n);
                            }
                        });

                        if (currMese == 1)
                        {
                            currMese = 12;
                            currAnno--;
                        }
                        else
                        {
                            currMese--;
                        }
                    }
                    WindowWrapper.Current().Dispatcher.Dispatch(() =>
                    {
                        NewsPageViewModel.Instance.IsUpdatingNews = false;
                    });
                    return new LoadMoreItemsResult() { Count = count };

                }).AsAsyncOperation<LoadMoreItemsResult>();
            }

            public void Reset()
            {
                Clear();
                DateTime now = DateTime.Now;
                currAnno = now.Year;
                currMese = now.Month;
            }
        }
    }
}
