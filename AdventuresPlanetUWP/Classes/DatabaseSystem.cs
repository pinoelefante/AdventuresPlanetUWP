using AdventuresPlanetUWP.Classes.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLitePCL;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace AdventuresPlanetUWP.Classes
{
    class DatabaseSystem
    {
        private static string DBFile = "AdventuresPlanet.sqlite";
        private static DatabaseSystem singleton;
        private SQLiteConnection conn;
        private static readonly string TABLE_NEWS = "news",
            TABLE_RECENSIONI = "recensioni",
            TABLE_SOLUZIONI = "soluzioni",
            TABLE_PODCAST = "podcast",
            TABLE_GALLERIE = "gallerie";
        private static readonly string QUERY_NEWS = $"CREATE TABLE IF NOT EXISTS {TABLE_NEWS} (id INTEGER PRIMARY KEY AUTOINCREMENT, link TEXT UNIQUE, titolo TEXT, anteprima TEXT, testo TEXT DEFAULT '', testoRich TEXT DEFAULT '', data TEXT DEFAULT '', img TEXT, meselink TEXT NOT NULL)", 
            QUERY_PODCAST = $"CREATE TABLE IF NOT EXISTS {TABLE_PODCAST} (link TEXT PRIMARY KEY, titolo TEXT NOT NULL, data TEXT, stagione INTEGER, episodio INTEGER, descrizione TEXT DEFAULT '', immagine TEXT) WITHOUT ROWID;",
            QUERY_RECENSIONI = $"CREATE TABLE IF NOT EXISTS {TABLE_RECENSIONI} (id TEXT PRIMARY KEY, nome TEXT NOT NULL, autore TEXT DEFAULT '', voto TEXT DEFAULT '', votoUtenti TEXT DEFAULT '', link TEXT NOT NULL, testoBreve TEXT DEFAULT '', testo TEXT DEFAULT '', testoRich TEXT DEFAULT '', store TEXT DEFAULT '') WITHOUT ROWID;",
            QUERY_SOLUZIONI = $"CREATE TABLE IF NOT EXISTS {TABLE_SOLUZIONI} (id TEXT PRIMARY KEY, nome TEXT NOT NULL, autore TEXT DEFAULT '', link TEXT NOT NULL, soluzione TEXT DEFAULT '', soluzioneRich TEXT DEFAULT '', store TEXT DEFAULT '') WITHOUT ROWID;",
            QUERY_GALLERIE = $"CREATE TABLE IF NOT EXISTS {TABLE_GALLERIE} (id TEXT PRIMARY KEY, nome TEXT NOT NULL)";
        
        public static DatabaseSystem Instance
        {
            get
            {
                if (singleton == null)
                    singleton = new DatabaseSystem();
                return singleton;
            }
        }

        public void creaDB()
        {
            using (var st = conn.Prepare(QUERY_NEWS))
                st.Step();
            using (var st = conn.Prepare(QUERY_RECENSIONI))
                st.Step();
            using (var st = conn.Prepare(QUERY_SOLUZIONI))
                st.Step();
            using (var st = conn.Prepare(QUERY_PODCAST))
                st.Step();
            using (var st = conn.Prepare(QUERY_GALLERIE))
                st.Step();

            aggiornaDB();
        }
        public bool insertNews(News n)
        {
            string query = $"INSERT INTO {TABLE_NEWS} (link,titolo,anteprima,testo,testoRich,data,img,meselink) VALUES (?,?,?,?,?,?,?,?)";
            using (var st = conn.Prepare(query))
            {
                st.Bind(1, n.Link);
                st.Bind(2, n.Titolo);
                st.Bind(3, n.AnteprimaNews);
                st.Bind(4, n.CorpoNews);
                st.Bind(5, Rich_ListToText(n.CorpoNewsRich));
                st.Bind(6, n.DataPubblicazione);
                st.Bind(7, n.Immagine);
                st.Bind(8, n.MeseLink);
                SQLiteResult res = st.Step();
                if (res == SQLiteResult.DONE)
                {
                    n.Id = (int)st.Connection.LastInsertRowId();
                }
                Debug.WriteLine("Insert news = " + res.ToString());
                return res == SQLiteResult.DONE;
            }
        }

        public void updateDettagliNews(News n)
        {
            string query = $"UPDATE {TABLE_NEWS} SET testo = ? , testoRich = ? WHERE id = ?";
            using (var st = conn.Prepare(query))
            {
                st.Bind(1, n.CorpoNews);
                st.Bind(2, Rich_ListToText(n.CorpoNewsRich));
                st.Bind(3, n.Id);
                SQLiteResult res = st.Step();
            }
        }
        public List<News> insertNews(List<News> news, bool fromEnd = true)
        {
            List<News> inserite = new List<News>(news.Count);
            if (fromEnd)
            {
                for (int i = news.Count - 1; i >= 0; i--)
                    if (insertNews(news[i]))
                        inserite.Insert(0, news[i]);
            }
            else
            {
                for (int i = 0; i < news.Count; i++)
                    if (insertNews(news[i]))
                        inserite.Add(news[i]);
            }
            return inserite;
        }
        public void deleteNewsByMeseLink(string link)
        {
            string query = $"DELETE FROM {TABLE_NEWS} WHERE meselink = ?";
            using (var st = conn.Prepare(query))
            {
                st.Bind(1, link);
                st.Step();
            }
        }
        public void deleteNewsByLink(string link)
        {
            string query = $"DELETE FROM {TABLE_NEWS} WHERE link = ?";
            using (var st = conn.Prepare(query))
            {
                st.Bind(1, link);
                st.Step();
            }
        }
        public List<News> selectNewsByMeseLink(string meseLink)
        {
            string query = $"SELECT * FROM {TABLE_NEWS} WHERE meselink = '{meseLink}' ORDER BY id DESC";
            List<News> list = new List<News>();
            using (var st = conn.Prepare(query))
            {
                while (st.Step() == SQLiteResult.ROW)
                {
                    News news = parseNews(st, false);
                    Debug.WriteLine(news.Titolo + " id = " + news.Id);
                    list.Add(news);
                }
            }
            return list;
        }
        public News selectNews(News news, bool complete = false)
        {
            string query = $"SELECT * FROM {TABLE_NEWS} WHERE id = {news.Id}";
            using (var st = conn.Prepare(query))
            {
                if (st.Step() == SQLiteResult.ROW)
                {
                    return parseNews(st, complete, news);
                }
            }
            return null;
        }
        private News parseNews(ISQLiteStatement st, Boolean complete = true, News news = null)
        {
            var id = st.GetInteger("id");
            var link = st.GetText("link");
            var titolo = st.GetText("titolo");
            var anteprima = st.GetText("anteprima");
            var img = st.GetText("img");
            var mese = st.GetText("meselink");
            var data = st.GetText("data");

            if (news == null)
                news = new News();
            news.Id = (int)id;
            news.AnteprimaNews = anteprima;
            news.Immagine = img;
            news.Link = link;
            news.Titolo = titolo;
            news.MeseLink = mese;
            news.DataPubblicazione = data;

            if (complete)
            {
                var testo = st.GetText("testo");
                var testoR = st.GetText("testoRich");
                news.CorpoNews = testo;
                news.CorpoNewsRich = Rich_TextToList(testoR);
            }
            return news;
        }

        private void aggiornaDB()
        {
            switch (Settings.Instance.DatabaseVersion + 1)
            {
                case 2:
                case 3:
                case 4:
                default:
                    return;
            }
        }
        public void Connect()
        {
            conn = new SQLiteConnection(DBFile);
            creaDB();
        }
        public void Disconnect()
        {
            conn.Dispose();
            conn = null;
        }
        public bool loadDettagliRecensione(RecensioneItem item)
        {
            string query = $"SELECT votoUtenti,testoBreve,testo,testoRich,store FROM {TABLE_RECENSIONI} WHERE id = ?";
            using (var stmt = conn.Prepare(query))
            {
                stmt.Bind(1, item.Id);
                if (stmt.Step() == SQLiteResult.ROW)
                {
                    var votoU = stmt.GetText("votoUtenti") ?? string.Empty;
                    var testoB = stmt.GetText("testoBreve") ?? string.Empty;
                    var testo = stmt.GetText("testo") ?? string.Empty;
                    var testoRich = stmt.GetText("testoRich") ?? string.Empty;
                    var store = stmt.GetText("store") ?? string.Empty;

                    item.VotoUtentiText = votoU;
                    item.TestoBreve = testoB;
                    item.Testo = testo;
                    item.TestoRich = Rich_TextToList(testoRich);
                    item.LinkStore = store;
                    return true;
                }
            }
            return false;
        }
        public List<RecensioneItem> selectAllRecensioniLite()
        {
            string query = $"SELECT nome,autore,voto,link FROM {TABLE_RECENSIONI} ORDER BY nome ASC";
            List<RecensioneItem> list = new List<RecensioneItem>();
            using (var stmt = conn.Prepare(query))
            {
                while (stmt.Step() == SQLiteResult.ROW)
                {
                    var nome = stmt.GetText("nome");
                    var autore = stmt.GetText("autore");
                    var voto = stmt.GetText("voto");
                    var link = stmt.GetText("link");
                    RecensioneItem item = new RecensioneItem(nome, autore, voto, link);
                    list.Add(item);
                }
            }
            return list;
        }

        public RecensioneItem selectRecensione(string id)
        {
            string query = $"SELECT * FROM {TABLE_RECENSIONI} WHERE id = ?";
            using (var stmt = conn.Prepare(query))
            {
                stmt.Bind(1, id);
                while (stmt.Step() == SQLiteResult.ROW)
                {
                    var idR = stmt.GetText("id");
                    var nome = stmt.GetText("nome");
                    var autore = stmt.GetText("autore");
                    var voto = stmt.GetText("voto");
                    var votoU = stmt.GetText("votoUtenti") ?? string.Empty;
                    var link = stmt.GetText("link") ?? string.Empty;
                    var testoB = stmt.GetText("testoBreve") ?? string.Empty;
                    var testo = stmt.GetText("testo") ?? string.Empty;
                    var testoRich = stmt.GetText("testoRich") ?? string.Empty;
                    var store = stmt.GetText("store") ?? string.Empty;
                    RecensioneItem item = new RecensioneItem(nome, autore, voto, link);
                    item.Id = idR;
                    item.VotoUtentiText = votoU;
                    item.TestoBreve = testoB;
                    item.Testo = testo;
                    item.TestoRich = Rich_TextToList(testoRich);
                    item.LinkStore = store;
                    return item;
                }
            }
            return null;
        }
        public Boolean insertRecensione(RecensioneItem rec)
        {
            string query = $"INSERT INTO {TABLE_RECENSIONI} (id,nome,autore,voto,votoUtenti,link,testoBreve,testo,testoRich,store) VALUES (?,?,?,?,?,?,?,?,?,?)";
            try
            {
                using (var stmt = conn.Prepare(query))
                {
                    stmt.Bind(1, rec.Id);
                    stmt.Bind(2, rec.Titolo);
                    stmt.Bind(3, rec.AutoreText);
                    stmt.Bind(4, rec.VotoText);
                    stmt.Bind(5, rec.VotoUtentiText);
                    stmt.Bind(6, rec.Link);
                    stmt.Bind(7, rec.TestoBreve);
                    stmt.Bind(8, rec.Testo);
                    stmt.Bind(9, Rich_ListToText(rec.TestoRich));
                    stmt.Bind(10, rec.LinkStore);
                    stmt.Step();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString() + "\n" + e.StackTrace + "\n" + e.Source + "\n" + e.Message);
            }

            return true;
        }
        private bool multiInsertRecensione(List<RecensioneItem> list)
        {
            if (list.Count == 0)
                return true;
            /* Capacità builder
            57 lunghezza iniziale (arrotondato a 60)
            
            140 lunghezza media in byte di una recensione
            17 caratteri tra parentesi e virgole
            157 (arrotondato a 160)

            il tutto calcolato sulla lunghezza della lista +5 elementi vuoti per non correre il rischio di dover allocare altro spazio
            */
            StringBuilder builder = new StringBuilder($"INSERT INTO {TABLE_RECENSIONI} (id,nome,autore,voto,link) VALUES ", 60 + (list.Count + 5) * 160);

            for (int i = 0; i < list.Count - 1; i++)
            {
                RecensioneItem item = list[i];
                builder.Append($"(\"{item.Id}\",\"{item.Titolo}\",\"{item.AutoreText}\",\"{item.VotoText}\",\"{item.Link}\"),");
            }
            RecensioneItem ultimo = list[list.Count - 1];
            builder.Append($"(\"{ultimo.Id}\",\"{ultimo.Titolo}\",\"{ultimo.AutoreText}\",\"{ultimo.VotoText}\",\"{ultimo.Link}\")");

            //Debug.WriteLine(builder.ToString());
            try
            {
                using (var st = conn.Prepare(builder.ToString()))
                {
                    SQLiteResult res = st.Step();
                    Debug.WriteLine("multiinsert rec = " + res.ToString());
                    if (res == SQLiteResult.DONE)
                        return true;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            return false;
        }
        public Boolean insertRecensione(List<RecensioneItem> l, bool inserisciSingolarmente = false)
        {
            if (inserisciSingolarmente)
            {
                int fail = 0;
                foreach (RecensioneItem i in l)
                {
                    if (!insertRecensione(i))
                    {
                        fail++;
                        Debug.WriteLine("insertRecensione error: " + i.Titolo + " non inserito nel database");
                    }
                }
                return fail != l.Count;
            }
            else
                return multiInsertRecensione(l);
        }
        public void updateRecensione(RecensioneItem rec)
        {
            string query = $"UPDATE {TABLE_RECENSIONI} SET id=? , nome = ? , autore = ? , voto = ?, votoUtenti = ? , link = ? , testoBreve = ? , testo = ? , testoRich = ? , store = ? WHERE id = ?";
            using (var st = conn.Prepare(query))
            {
                st.Bind(1, rec.Id);
                st.Bind(2, rec.Titolo);
                st.Bind(3, rec.AutoreText);
                st.Bind(4, rec.VotoText);
                st.Bind(5, rec.VotoUtentiText);
                st.Bind(6, rec.Link);
                st.Bind(7, rec.TestoBreve);
                st.Bind(8, rec.Testo);
                st.Bind(9, Rich_ListToText(rec.TestoRich));
                st.Bind(10, rec.LinkStore);
                st.Bind(11, rec.Id);
                SQLiteResult res = st.Step();
            }
        }

        private string Rich_ListToText(List<string> list)
        {
            string text = String.Empty;
            for (int i = 0; list != null && i < list.Count; i++)
            {
                text += list[i] + "\n";
            }
            return text;
        }
        private List<string> Rich_TextToList(string text)
        {
            string[] s = text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return s.ToList();
        }
        public List<PodcastItem> selectAllPodcast()
        {
            string query = $"SELECT * FROM {TABLE_PODCAST} ORDER BY stagione DESC, episodio DESC";
            List<PodcastItem> podcast = new List<PodcastItem>();
            using (var st = conn.Prepare(query))
            {
                while (st.Step() == SQLiteResult.ROW)
                {
                    var link = st.GetText("link");
                    var titolo = st.GetText("titolo");
                    var data = st.GetText("data");
                    var descr = st.GetText("descrizione");
                    var stag = st.GetInteger("stagione");
                    var epis = st.GetInteger("episodio");
                    var img = st.GetText("immagine");
                    PodcastItem p = new PodcastItem(titolo, data, link, (int)stag, (int)epis) { Immagine = img};
                    p.Descrizione = descr;
                    podcast.Add(p);
                }
            }
            return podcast;
        }
        public ObservableCollection<PodcastItem> selectAllPodcastOb()
        {
            string query = $"SELECT * FROM {TABLE_PODCAST} ORDER BY stagione DESC, episodio DESC";
            ObservableCollection<PodcastItem> podcast = new ObservableCollection<PodcastItem>();
            using (var st = conn.Prepare(query))
            {
                while (st.Step() == SQLiteResult.ROW)
                {
                    var link = st.GetText("link");
                    var titolo = st.GetText("titolo");
                    var data = st.GetText("data");
                    var descr = st.GetText("descrizione");
                    var stag = st.GetInteger("stagione");
                    var epis = st.GetInteger("episodio");
                    var img = st.GetText("immagine");
                    PodcastItem p = new PodcastItem(titolo, data, link, (int)stag, (int)epis) { Immagine = img};
                    p.Descrizione = descr;
                    podcast.Add(p);
                }
            }
            return podcast;
        }
        public void insertPodcast(PodcastItem pod)
        {
            string query = $"INSERT INTO {TABLE_PODCAST} (link,titolo,data,stagione,episodio,descrizione,immagine) VALUES (?,?,?,?,?,?,?)";
            using (var st = conn.Prepare(query))
            {
                st.Bind(1, pod.Link);
                st.Bind(2, pod.Titolo);
                st.Bind(3, pod.Data);
                st.Bind(4, pod.Stagione);
                st.Bind(5, pod.Episodio);
                st.Bind(6, pod.Descrizione);
                st.Bind(7, pod.Immagine);
                st.Step();
            }
        }
        public void insertPodcast(List<PodcastItem> l)
        {
            foreach (PodcastItem pod in l)
            {
                insertPodcast(pod);
            }
        }
        public void insertSoluzione(SoluzioneItem sol)
        {
            string query = $"INSERT INTO {TABLE_SOLUZIONI} (id,nome,autore,link,soluzione,soluzioneRich,store) VALUES (?,?,?,?,?,?,?)";
            using (var st = conn.Prepare(query))
            {
                st.Bind(1, sol.Id);
                st.Bind(2, sol.Titolo);
                st.Bind(3, sol.AutoreText);
                st.Bind(4, sol.Link);
                st.Bind(5, sol.Testo);
                st.Bind(6, Rich_ListToText(sol.TestoRich));
                st.Bind(7, sol.LinkStore);
                st.Step();
            }
        }
        public Boolean multiInsertSoluzione(List<SoluzioneItem> list)
        {
            if (list.Count == 0)
                return true;

            StringBuilder builder = new StringBuilder($"INSERT INTO {TABLE_SOLUZIONI} (id,nome,autore,link) VALUES ", 55 + (list.Count + 5) * 150);
            for (int i = 0; i < list.Count - 1; i++)
            {
                SoluzioneItem sol = list[i];
                builder.Append($"(\"{sol.Id}\",\"{sol.Titolo}\",\"{sol.AutoreText}\",\"{sol.Link}\"),");
            }
            SoluzioneItem sol1 = list[list.Count - 1];
            builder.Append($"(\"{sol1.Id}\",\"{sol1.Titolo}\",\"{sol1.AutoreText}\",\"{sol1.Link}\")");
            try
            {
                using (var st = conn.Prepare(builder.ToString()))
                {
                    SQLiteResult res = st.Step();
                    Debug.WriteLine("multiinsert sol = " + res.ToString());
                    if (res == SQLiteResult.DONE)
                        return true;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            return false;
        }
        public void insertSoluzione(List<SoluzioneItem> l, bool inserisciSingolarmente = false)
        {
            if (inserisciSingolarmente)
            {
                foreach (SoluzioneItem sol in l)
                    insertSoluzione(sol);
            }
            else
                multiInsertSoluzione(l);
        }

        public List<SoluzioneItem> selectAllSoluzioni()
        {
            string query = $"SELECT * FROM {TABLE_SOLUZIONI} ORDER BY nome ASC";
            List<SoluzioneItem> l = new List<SoluzioneItem>();
            using (var st = conn.Prepare(query))
            {
                while (st.Step() == SQLiteResult.ROW)
                {
                    var id = st.GetText("id");
                    var nome = st.GetText("nome");
                    var autore = st.GetText("autore");
                    var link = st.GetText("link");
                    var sol = st.GetText("soluzione");
                    var solR = st.GetText("soluzioneRich");
                    var store = st.GetText("store");
                    SoluzioneItem item = new SoluzioneItem(nome, autore, link);
                    item.Id = id;
                    item.Testo = sol;
                    item.TestoRich = Rich_TextToList(solR);
                    item.LinkStore = store;
                    l.Add(item);
                }
            }
            return l;
        }
        public bool loadDettagliSoluzione(SoluzioneItem item)
        {
            string query = $"SELECT soluzione,soluzioneRich,store FROM {TABLE_SOLUZIONI} WHERE id = ?";
            List<SoluzioneItem> l = new List<SoluzioneItem>();
            using (var st = conn.Prepare(query))
            {
                st.Bind(1, item.Id);
                if (st.Step() == SQLiteResult.ROW)
                {
                    var sol = st.GetText("soluzione") ?? string.Empty;
                    var solR = st.GetText("soluzioneRich") ?? string.Empty;
                    var store = st.GetText("store") ?? string.Empty;
                    item.Testo = sol;
                    item.TestoRich = Rich_TextToList(solR);
                    item.LinkStore = store;
                    l.Add(item);
                    return true;
                }
            }
            return false;
        }
        public List<SoluzioneItem> selectAllSoluzioniLite()
        {
            string query = $"SELECT nome,autore,link FROM {TABLE_SOLUZIONI} ORDER BY nome ASC";
            List<SoluzioneItem> l = new List<SoluzioneItem>();
            using (var st = conn.Prepare(query))
            {
                while (st.Step() == SQLiteResult.ROW)
                {
                    var nome = st.GetText("nome");
                    var autore = st.GetText("autore");
                    var link = st.GetText("link");
                    SoluzioneItem item = new SoluzioneItem(nome, autore, link);
                    l.Add(item);
                }
            }
            return l;
        }
        public SoluzioneItem selectSoluzione(string id)
        {
            string query = $"SELECT * FROM {TABLE_SOLUZIONI} WHERE id = ?";
            using (var st = conn.Prepare(query))
            {
                st.Bind(1, id);
                if (st.Step() == SQLiteResult.ROW)
                {
                    var idS = st.GetText("id");
                    var nome = st.GetText("nome");
                    var autore = st.GetText("autore");
                    var link = st.GetText("link");
                    var sol = st.GetText("soluzione") ?? string.Empty;
                    var solR = st.GetText("soluzioneRich") ?? string.Empty;
                    var store = st.GetText("store") ?? string.Empty;
                    SoluzioneItem item = new SoluzioneItem(nome, autore, link);
                    item.Id = idS;
                    item.Testo = sol;
                    item.TestoRich = Rich_TextToList(solR);
                    item.LinkStore = store;
                    return item;
                }
            }
            return null;
        }
        public void updateSoluzione(SoluzioneItem sol)
        {
            string query = $"UPDATE {TABLE_SOLUZIONI} SET id = ? , nome = ? , autore = ? , link = ? , soluzione = ? , soluzioneRich = ? , store = ? WHERE id = ?";
            using (var st = conn.Prepare(query))
            {
                st.Bind(1, sol.Id);
                st.Bind(2, sol.Titolo);
                st.Bind(3, sol.AutoreText);
                st.Bind(4, sol.Link);
                st.Bind(5, sol.Testo);
                st.Bind(6, Rich_ListToText(sol.TestoRich));
                st.Bind(7, sol.LinkStore);
                st.Bind(8, sol.Id);
                st.Step();
            }
        }
        public void insertGalleria(string nome, string id)
        {
            string query = $"INSERT INTO {TABLE_GALLERIE} (id, nome) VALUES (\"{id}\",\"{nome}\")";
            using (var st = conn.Prepare(query))
                st.Step();
        }
        public bool multiInsertGallerie(List<KeyValuePair<string, string>> lista)
        {
            if (lista.Count == 0)
                return true;
            StringBuilder builder = new StringBuilder($"INSERT INTO {TABLE_GALLERIE} (id,nome) VALUES ", 40 + (lista.Count * 32));
            for (int i = 0; i < lista.Count - 1; i++)
            {
                var item = lista[i];
                builder.Append($"(\"{item.Key}\",\"{item.Value}\"),");
            }
            KeyValuePair<string, string> last = lista.Last();
            builder.Append($"(\"{last.Key}\",\"{last.Value}\")");

            using (var st = conn.Prepare(builder.ToString()))
            {
                try
                {
                    if (st.Step() == SQLiteResult.DONE)
                        return true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            return false;
        }
        public string selectTitoloGalleria(string id)
        {
            string query = $"SELECT nome FROM {TABLE_GALLERIE} WHERE id = ?";
            using(var st = conn.Prepare(query))
            {
                st.Bind(1, id);
                if(st.Step() == SQLiteResult.ROW)
                {
                    string titolo = st.GetText("nome");
                    return titolo;
                }
            }
            return string.Empty;
        }
        public bool hasGalleria(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;
            return true;
        }
        private void dropTable(string table)
        {
            string dropQ = $"DROP TABLE IF EXISTS {table}";
            using (var st = conn.Prepare(dropQ))
            {
                st.Step();
            }
        }
        public void dropDB()
        {
            dropTable(TABLE_NEWS);
            dropTable(TABLE_RECENSIONI);
            dropTable(TABLE_SOLUZIONI);
            dropTable(TABLE_PODCAST);
            dropTable(TABLE_GALLERIE);
            vacuum();
            creaDB();
        }
        public void cleanTables()
        {
            cleanTable(TABLE_NEWS);
            cleanTable(TABLE_RECENSIONI);
            cleanTable(TABLE_SOLUZIONI);
            cleanTable(TABLE_PODCAST);
            cleanTable(TABLE_GALLERIE);
            vacuum();
        }
        private void cleanTable(string table)
        {
            string cleanT = $"DELETE FROM {table}";
            using (var st = conn.Prepare(cleanT))
            {
                st.Step();
            }
        }
        public void cleanNews()
        {
            dropTable(TABLE_NEWS);
            using (var st = conn.Prepare(QUERY_NEWS))
                st.Step();
        }
        public void cleanRecensioni()
        {
            dropTable(TABLE_RECENSIONI);
            using (var st = conn.Prepare(QUERY_RECENSIONI))
                st.Step();
        }
        public void cleanSoluzioni()
        {
            dropTable(TABLE_SOLUZIONI);
            using (var st = conn.Prepare(QUERY_SOLUZIONI))
                st.Step();
        }
        public void cleanPodcast()
        {
            dropTable(TABLE_PODCAST);
            using (var st = conn.Prepare(QUERY_PODCAST))
                st.Step();
        }
        public void cleanGallerie()
        {
            dropTable(TABLE_GALLERIE);
            using (var st = conn.Prepare(QUERY_GALLERIE))
                st.Step();
        }
        public void vacuum()
        {
            using (var st = conn.Prepare("VACUUM"))
                st.Step();
        }
    }
}
