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
        public static DatabaseSystem Instance
        {
            get
            {
                if (singleton == null)
                    singleton = new DatabaseSystem();
                return singleton;
            }
        }

        private void creaDB()
        {
            string news = "CREATE TABLE IF NOT EXISTS news (id INTEGER PRIMARY KEY AUTOINCREMENT, link TEXT UNIQUE, titolo TEXT, anteprima TEXT, testo TEXT DEFAULT '', testoRich TEXT DEFAULT '', data TEXT DEFAULT '', img TEXT, meselink TEXT NOT NULL)";
            string podcast = "CREATE TABLE IF NOT EXISTS podcast (link TEXT PRIMARY KEY, titolo TEXT NOT NULL, data TEXT, stagione INTEGER, episodio INTEGER, descrizione TEXT DEFAULT '', immagine TEXT) WITHOUT ROWID;";
            string recensioni = "CREATE TABLE IF NOT EXISTS recensioni (id TEXT PRIMARY KEY, nome TEXT NOT NULL, autore TEXT DEFAULT '', voto TEXT DEFAULT '', votoUtenti TEXT DEFAULT '', link TEXT NOT NULL, testoBreve TEXT DEFAULT '', testo TEXT DEFAULT '', testoRich TEXT DEFAULT '', store TEXT DEFAULT '') WITHOUT ROWID;";
            string soluzioni = "CREATE TABLE IF NOT EXISTS soluzioni (id TEXT PRIMARY KEY, nome TEXT NOT NULL, autore TEXT DEFAULT '', link TEXT NOT NULL, soluzione TEXT DEFAULT '', soluzioneRich TEXT DEFAULT '', store TEXT DEFAULT '') WITHOUT ROWID;";
            string preferiti = "CREATE TABLE IF NOT EXISTS preferiti (id TEXT PRIMARY KEY) WITHOUT ROWID";

            using (var st = conn.Prepare(podcast))
                st.Step();
            using (var st = conn.Prepare(recensioni))
                st.Step();
            using (var st = conn.Prepare(soluzioni))
                st.Step();
            using (var st = conn.Prepare(preferiti))
                st.Step();
            using (var st = conn.Prepare(news))
                st.Step();
            aggiornaDB();
        }
        public bool insertNews(News n)
        {
            string query = "INSERT INTO news (link,titolo,anteprima,testo,testoRich,data,img,meselink) VALUES (?,?,?,?,?,?,?,?)";
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
            string query = "UPDATE news SET testo = ? , testoRich = ? WHERE id = ?";
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
            string query = "DELETE FROM news WHERE meselink = ?";
            using (var st = conn.Prepare(query))
            {
                st.Bind(1, link);
                st.Step();
            }
        }
        public void deleteNewsByLink(string link)
        {
            string query = "DELETE FROM news WHERE link = ?";
            using (var st = conn.Prepare(query))
            {
                st.Bind(1, link);
                st.Step();
            }
        }
        public List<News> selectNewsByMeseLink(string meseLink)
        {
            string query = "SELECT * FROM news WHERE meselink = '" + meseLink + "' ORDER BY id DESC";
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
        public List<News> selectNews(int limit = 10, int from = 0)
        {
            string query = "SELECT * FROM news ORDER BY id DESC LIMIT " + limit + (from > 0 ? " WHERE id<" + from : "");
            List<News> list = new List<News>(limit > 0 ? limit : 10);
            using (var st = conn.Prepare(query))
            {
                while (st.Step() == SQLiteResult.ROW)
                {
                    News news = parseNews(st);
                    Debug.WriteLine(news.Titolo + " id = " + news.Id);
                    list.Insert(0, news);
                }
            }
            return list;
        }
        public News selectNews(News news, bool complete = false)
        {
            string query = "SELECT * FROM news WHERE id = " + news.Id;
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
        public void deleteDatabase()
        {

        }
        public List<RecensioneItem> selectAllRecensioni()
        {
            string query = "SELECT * FROM recensioni ORDER BY nome ASC";
            List<RecensioneItem> list = new List<RecensioneItem>();
            using (var stmt = conn.Prepare(query))
            {
                while (stmt.Step() == SQLiteResult.ROW)
                {
                    var id = stmt.GetText("id");
                    var nome = stmt.GetText("nome");
                    var autore = stmt.GetText("autore");
                    var voto = stmt.GetText("voto");
                    var votoU = stmt.GetText("votoUtenti");
                    var link = stmt.GetText("link");
                    var testoB = stmt.GetText("testoBreve");
                    var testo = stmt.GetText("testo");
                    var testoRich = stmt.GetText("testoRich");
                    var store = stmt.GetText("store");
                    RecensioneItem item = new RecensioneItem(nome, autore, voto, link);
                    item.Id = id;
                    item.VotoUtentiText = votoU;
                    item.TestoBreve = testoB;
                    item.Testo = testo;
                    item.TestoRich = Rich_TextToList(testoRich);
                    item.LinkStore = store;
                    list.Add(item);
                }
            }
            return list;
        }
        public bool loadDettagliRecensione(RecensioneItem item)
        {
            string query = "SELECT votoUtenti,testoBreve,testo,testoRich,store FROM recensioni WHERE id = ?";
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
            string query = "SELECT nome,autore,voto,link FROM recensioni ORDER BY nome ASC";
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
            string query = "SELECT * FROM recensioni WHERE id = ?";
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
            string query = "INSERT INTO recensioni (id,nome,autore,voto,votoUtenti,link,testoBreve,testo,testoRich,store) VALUES (?,?,?,?,?,?,?,?,?,?)";
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
            StringBuilder builder = new StringBuilder("INSERT INTO recensioni (id,nome,autore,voto,link) VALUES ", 60 + (list.Count + 5) * 160);

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
            string query = "UPDATE recensioni SET id=? , nome = ? , autore = ? , voto = ?, votoUtenti = ? , link = ? , testoBreve = ? , testo = ? , testoRich = ? , store = ? WHERE id = ?";
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
            string query = "SELECT * FROM podcast ORDER BY stagione DESC, episodio DESC";
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
            string query = "SELECT * FROM podcast ORDER BY stagione DESC, episodio DESC";
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
            string query = "INSERT INTO podcast (link,titolo,data,stagione,episodio,descrizione,immagine) VALUES (?,?,?,?,?,?,?)";
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
            string query = "INSERT INTO soluzioni (id,nome,autore,link,soluzione,soluzioneRich,store) VALUES (?,?,?,?,?,?,?)";
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

            StringBuilder builder = new StringBuilder("INSERT INTO soluzioni (id,nome,autore,link) VALUES ", 55 + (list.Count + 5) * 150);
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
            string query = "SELECT * FROM soluzioni ORDER BY nome ASC";
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
            string query = "SELECT soluzione,soluzioneRich,store FROM soluzioni WHERE id = ?";
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
            string query = "SELECT nome,autore,link FROM soluzioni ORDER BY nome ASC";
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
            string query = "SELECT * FROM soluzioni WHERE id = ?";
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
            string query = "UPDATE soluzioni SET id = ? , nome = ? , autore = ? , link = ? , soluzione = ? , soluzioneRich = ? , store = ? WHERE id = ?";
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
        public void cleanDB()
        {
            Debug.WriteLine("clean db");
#if DEBUG
            Debug.WriteLine("Dropping tables...");
            dropDB();
#else
            Debug.WriteLine("Cleaning tables...");
            cleanTables();
#endif
        }
        private void dropTable(string table)
        {
            string dropQ = "DROP TABLE IF EXISTS " + table;
            using (var st = conn.Prepare(dropQ))
            {
                st.Step();
            }
        }
        public void dropDB()
        {
            dropTable("recensioni");
            dropTable("soluzioni");
            dropTable("podcast");
            dropTable("news");
            //dropTable("preferiti");
            creaDB();
        }
        public void cleanTables()
        {
            cleanTable("recensioni");
            cleanTable("soluzioni");
            cleanTable("podcast");
            cleanTable("news");
            //cleanTable("preferiti");
        }
        private void cleanTable(string table)
        {
            string cleanT = "DELETE FROM " + table;
            using (var st = conn.Prepare(cleanT))
            {
                st.Step();
            }
        }
        public void aggiungiPreferiti(string id)
        {
            string query = "INSERT INTO preferiti (id) VALUES (?)";
            using (var st = conn.Prepare(query))
            {
                st.Bind(1, id);
                st.Step();
            }
        }
        public void rimuoviPreferiti(string id)
        {
            string query = "DELETE FROM preferiti WHERE id = ?";
            using (var st = conn.Prepare(query))
            {
                st.Bind(1, id);
                st.Step();
            }
        }
        public Boolean isPreferiti(string id)
        {
            string query = "SELECT id FROM preferiti WHERE id = ?";
            using (var st = conn.Prepare(query))
            {
                st.Bind(1, id);
                if (st.Step() == SQLiteResult.ROW)
                    return true;
            }
            return false;
        }
        public List<EntryAvventura> selectAllPreferiti()
        {
            List<EntryAvventura> list = new List<EntryAvventura>();
            string query = "SELECT * FROM preferiti";
            using (var st = conn.Prepare(query))
            {
                while (st.Step() == SQLiteResult.ROW)
                {
                    var id = st.GetText("id");
                    RecensioneItem rec = selectRecensione(id);
                    SoluzioneItem sol = selectSoluzione(id);
                    EntryAvventura e = new EntryAvventura(rec, sol);
                    list.Add(e);
                }
            }
            return list.OrderBy(x => x.Titolo).ToList();
        }
    }
}
