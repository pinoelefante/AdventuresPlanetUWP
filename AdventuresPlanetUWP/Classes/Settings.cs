using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace AdventuresPlanetUWP.Classes
{
    public class Settings
    {
        private ApplicationDataContainer local_settings;
        private static Settings instance;
        public static Settings Instance
        {
            get
            {
                if (instance == null)
                    instance = new Settings();
                return instance;
            }
        }
        private Settings()
        {
            local_settings = ApplicationData.Current.LocalSettings;
            avvioApp();
            OnlineConfig = ReadOnlineConfig();
        }
        public int DimensioneFont 
        {
            get { return local_settings.Values["font_size"]!=null ? (int)local_settings.Values["font_size"] : 18; }
            set {
                Debug.WriteLine("Font size = " + value);
                local_settings.Values["font_size"] = value; } 
        }
        public Boolean FirstStart 
        { 
            get 
            {
                if (local_settings.Values["first_start"] == null)
                {
                    Debug.WriteLine("first_start = null");
                    return true;
                }
                Debug.WriteLine("first_start = "+local_settings.Values["first_start"].ToString());
                return (Boolean)local_settings.Values["first_start"];
            }
            set
            {
                Debug.WriteLine("Setto il valore di first_start a " + value.ToString());
                local_settings.Values["first_start"] = value;
            }
        }
        public Boolean LetItSnow
        {
            get
            {
                if (local_settings.Values["letitsnow"] == null)
                {
                    return true;
                }
                return (Boolean)local_settings.Values["letitsnow"];
            }
            set
            {
                local_settings.Values["letitsnow"] = value;
            }
        }
        internal void reset()
        {
            local_settings.Values.Clear();
        }

        public int SoluzionePosition(string id)
        {
            if (local_settings.Values[$"sol_{id}"] == null)
            {
                //Debug.WriteLine("sol_"+id+" = null");
                return 0;
            }
            int pos = (int)local_settings.Values[$"sol_{id}"];
            //Debug.WriteLine("sol_" + id + "=" + pos);
            return pos;
        }
        public int RecensionePosition(string id)
        {
            if (local_settings.Values[$"rec_{id}"] == null)
                return 0;
            int pos = (int)local_settings.Values[$"rec_{id}"];
            return pos;
        }
        public void SaveSoluzionePosition(string id, int pos)
        {
            local_settings.Values[$"sol_{id}"] = pos;
        }
        public void SavePodcastPosition(string filename, long position)
        {
            local_settings.Values[$"pod_{filename}"] = position;
        }
        public long PodcastPosition(string filename)
        {
            if (local_settings.Values[$"pod_{filename}"] == null)
                return 0;
            long pos = (long)local_settings.Values[$"pod_{filename}"];
            return pos;
        }
        public Int32 DatabaseVersion
        {
            get
            {
                if (local_settings.Values["db_version"] == null)
                    return 1;
                return (Int32)local_settings.Values["db_version"];
            }
            set
            {
                local_settings.Values["db_version"] = value;
            }
        }
        public int NumeroAvvii {
            get
            {
                if (local_settings.Values["avvii"] == null)
                    return 0;
                return (Int32)local_settings.Values["avvii"];
            }
            set
            {
                local_settings.Values["avvii"] = value;
            }
        }
        public void avvioApp()
        {
            NumeroAvvii++;
        }
        public long LastRecensioniUpdate
        {
            get
            {
                if (local_settings.Values["recensioni_last_update"] == null)
                    return 0;
                return (long)local_settings.Values["recensioni_last_update"];
            }
            set
            {
                local_settings.Values["recensioni_last_update"] = value;
            }
        }
        public long LastGallerieUpdate
        {
            get
            {
                if (local_settings.Values["gallerie_last_update"] == null)
                    return 0;
                return (long)local_settings.Values["gallerie_last_update"];
            }
            set
            {
                local_settings.Values["gallerie_last_update"] = value;
            }
        }
        public async Task<bool> IsRecensioniUpdated()
        {
            if (!_isOnlineConfigRead)
                await OnlineConfig;
            if (_OnlineSettings != null)
            {
                if (getUnixTimeStamp() - LastRecensioniUpdate > _OnlineSettings.RecensioniUpdateTime)
                    return false;
            }
            else
            {
                if (getUnixTimeStamp() - LastRecensioniUpdate > 86400)
                    return false;
            }
            return true;
        }
        public async Task<bool> IsGallerieUpdated()
        {
            if (!_isOnlineConfigRead)
                await OnlineConfig;
            if (_OnlineSettings != null)
            {
                if (getUnixTimeStamp() - LastGallerieUpdate > _OnlineSettings.GallerieUpdateTime)
                    return false;
            }
            else
            {
                if (getUnixTimeStamp() - LastGallerieUpdate > 86400)
                    return false;
            }
            return true;
        }
        public long LastSoluzioniUpdate
        {
            get
            {
                if (local_settings.Values["soluzioni_last_update"] == null)
                    return 0;
                return (long)local_settings.Values["soluzioni_last_update"];
            }
            set
            {
                local_settings.Values["soluzioni_last_update"] = value;
            }
        }
        public async Task<bool> IsSoluzioniUpdated()
        {
            if (!_isOnlineConfigRead)
                await OnlineConfig;
            if (_OnlineSettings != null)
            {
                if (getUnixTimeStamp() - LastSoluzioniUpdate > _OnlineSettings.SoluzioniUpdateTime)
                    return false;
            }
            else
            {
                if (getUnixTimeStamp() - LastSoluzioniUpdate > 86400)
                    return false;
            }
            return true;
        }
        public Boolean UpdateRecensioniManualmente
        {
            get
            {
                if (local_settings.Values["recensioni_update_manually"] == null)
                    return false;
                return (Boolean)local_settings.Values["recensioni_update_manually"];
            }
            set
            {
                local_settings.Values["recensioni_update_manually"] = value;
            }
        }
        public Boolean UpdateSoluzioniManualmente
        {
            get
            {
                if (local_settings.Values["soluzioni_update_manually"] == null)
                    return false;
                return (Boolean)local_settings.Values["soluzioni_update_manually"];
            }
            set
            {
                local_settings.Values["soluzioni_update_manually"] = value;
            }
        }
        public long LastPodcastUpdate
        {
            get
            {
                if (local_settings.Values["podcast_last_update"] == null)
                    return 0;
                return (long)local_settings.Values["podcast_last_update"];
            }
            set
            {
                local_settings.Values["podcast_last_update"] = value;
            }
        }
        public Boolean IsLoadImages
        {
            get
            {
                if (local_settings.Values["load_images"] == null)
                    return true;
                return (bool)local_settings.Values["load_images"];
            }
            set
            {
                local_settings.Values["load_images"] = value;
            }
        }
        public async Task<bool> IsPodcastUpdated()
        {
            if (!_isOnlineConfigRead)
                await OnlineConfig;
            if (_OnlineSettings != null)
            {
                if (getUnixTimeStamp() - LastPodcastUpdate > _OnlineSettings.PodcastUpdateTime)
                    return false;
            }
            else
            {
                if (getUnixTimeStamp() - LastPodcastUpdate > 86400)
                    return false;
            }
            return true;
        }
        public long LastNewsUpdate
        {
            get
            {
                if (local_settings.Values["news_last_update"] == null)
                    return 0;
                return (long)local_settings.Values["news_last_update"];
            }
            set
            {
                local_settings.Values["news_last_update"] = value;
            }
        }
        public long TimeUpdateNews
        {
            get
            {
                if (local_settings.Values["time_update_news"] == null)
                    return 3600;
                return (long)local_settings.Values["time_update_news"];
            }
            set
            {
                local_settings.Values["time_update_news"] = value;
            }
        }
        public Boolean IsNewsUpdated
        {
            get
            {
                if (getUnixTimeStamp() - LastNewsUpdate > TimeUpdateNews)
                    return false;
                return true;
            }
        }

        public bool AskToClose {
            get
            {
                if (local_settings.Values["ask_to_close"] == null)
                    return true;
                return (bool)local_settings.Values["ask_to_close"];
            }
            set
            {
                local_settings.Values["ask_to_close"] = value;
            }
        }

        public bool Votato {
            get {
                if (local_settings.Values["votato"] == null)
                    return false;
                return (Boolean)local_settings.Values["votato"];
            }
            set
            {
                local_settings.Values["votato"] = value;
            }
        }

        public Boolean isNewsMesePersistent(string link)
        {
            if (local_settings.Values[link] == null)
                return false;
            return (bool)local_settings.Values[link];
        }
        public void setNewsMesePersistent(string link, bool stato)
        {
            local_settings.Values[link] = stato;
        }
        public static long getUnixTimeStamp()
        {
            long unixTimestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return unixTimestamp;
        }
        public static long getUnixTimeStamp(int anno, int mese)
        {
            DateTime periodo = new DateTime(anno, mese, 1);
            long unixTimestamp = (long)(periodo.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return unixTimestamp;
        }
        public void SaveRecensionePosition(string id, int itemIndex)
        {
            local_settings.Values[$"rec_{id}"] = itemIndex;
        }
        public bool RicordaPosizioneSoluzioni
        {
            get
            {
                if (local_settings.Values["ricorda_posizione_soluzioni"] == null)
                    return true;
                return (Boolean)local_settings.Values["ricorda_posizione_soluzioni"];
            }
            set
            {
                local_settings.Values["ricorda_posizione_soluzioni"] = value;
            }
        }
        public bool RicordaPosizionePodcast
        {
            get
            {
                if (local_settings.Values["ricorda_posizione_podcast"] == null)
                    return false;
                return (Boolean)local_settings.Values["ricorda_posizione_podcast"];
            }
            set
            {
                local_settings.Values["ricorda_posizione_podcast"] = value;
            }
        }
        public bool RicordaPosizioneRecensioni
        {
            get
            {
                if (local_settings.Values["ricorda_posizione_recensioni"] == null)
                    return true;
                return (Boolean)local_settings.Values["ricorda_posizione_recensioni"];
            }
            set
            {
                local_settings.Values["ricorda_posizione_recensioni"] = value;
            }
        }
        public int QualitaVideoMax
        {
            get
            {
                if (local_settings.Values["youtube_qualita_max"] == null)
                    return 480;
                return (int)local_settings.Values["youtube_qualita_max"];
            }
            set
            {
                local_settings.Values["youtube_qualita_max"] = value;
            }
        }
        public YouTubeQuality YouTubeQualityMax()
        {
            switch (QualitaVideoMax)
            {
                case 144:
                    return YouTubeQuality.Quality144P;
                case 240:
                    return YouTubeQuality.Quality240P;
                case 360:
                    return YouTubeQuality.Quality360P;
                case 480:
                    return YouTubeQuality.Quality480P;
                case 720:
                    return YouTubeQuality.Quality720P;
                case 1080:
                    return YouTubeQuality.Quality1080P;
                case 2160:
                    return YouTubeQuality.Quality2160P;
            }
            return YouTubeQuality.Quality480P;
        }
        private Task OnlineConfig;
        private bool _isOnlineConfigRead = false;
        private OnlineSettings _OnlineSettings = new OnlineSettings();
        private async Task ReadOnlineConfig()
        {
            try
            {
                HttpClient http = new HttpClient();
                Stream configStream = await http.GetStreamAsync(new Uri("http://pinoelefante.altervista.org/avp_it/config.json"));
                DataContractJsonSerializer ds = new DataContractJsonSerializer(typeof(OnlineSettings));
                _OnlineSettings = (OnlineSettings)ds.ReadObject(configStream);
            }
            catch
            {
                
            }
            finally
            {
                _isOnlineConfigRead = true;
            }
            
        }
        public async Task<bool> IsAdsActive()
        {
            if (!_isOnlineConfigRead)
                await OnlineConfig;
            if (_OnlineSettings != null)
                return _OnlineSettings.AdsActive;
            else
                return true;
        }
        public class OnlineSettings
        {
            public bool AdsActive { get; set; } = true;
            public long RecensioniUpdateTime { get; set; } = 86400;
            public long SoluzioniUpdateTime { get; set; } = 86400;
            public long PodcastUpdateTime { get; set; } = 86400;
            public long GallerieUpdateTime { get; set; } = 86400;
        }
    }
}
