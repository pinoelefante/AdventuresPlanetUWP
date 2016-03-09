using AdventuresPlanetUWP.Classes.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;

namespace AdventuresPlanetUWP.Classes
{
    public class PodcastManager
    {
        private static PodcastManager _instance = new PodcastManager();
        private PodcastManager() { }
        public static PodcastManager Instance
        {
            get
            {
                return _instance;
            }
        }
        public void Init()
        {
            ValueSet set = new ValueSet()
            {
                {"Command", "Init" }
            };
            BackgroundMediaPlayer.SendMessageToBackground(set);
        }
        public async Task<bool> isDownloaded(string url)
        {
            string filename = getFileName(url);
            try {
                StorageFolder music = KnownFolders.MusicLibrary;
                StorageFolder podcastDir = await music.GetFolderAsync("Podcast");

                if (podcastDir == null)
                    return false;

                StorageFile destFile = await podcastDir.GetFileAsync(filename);
                if (destFile == null)
                {
                    return false;
                }
                else
                {
                    if ((await destFile.GetBasicPropertiesAsync()).Size == 0)
                        return false;
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
            return true;
        }
        public async Task<StorageFile> getLocalFile(string url)
        {
            string filename = getFileName(url);
            StorageFolder music = KnownFolders.MusicLibrary;
            StorageFolder podcastDir = await music.GetFolderAsync("Podcast");
            StorageFile destFile = await podcastDir.GetFileAsync(filename);
            return destFile;
        }
        private string getFileName(string url)
        {
            return url.Substring(url.LastIndexOf("/") + 1);
        }
        public async void Play(PodcastItem pod)
        {
            if (await isDownloaded(pod.Link))
                PlayOffline(pod);
            else
                PlayOnline(pod);
        }
        public void PlayOnline(PodcastItem pod)
        {
            //TODO send toast se offline
            Debug.WriteLine("Play online");
            ValueSet set = new ValueSet()
            {
                {"Command", "PlayOnline" },
                {"Thumb", pod.Immagine},
                {"Artist", "Calavera Cafè" },
                {"Title", pod.TitoloBG },
                {"Url", pod.Link }
            };
            BackgroundMediaPlayer.SendMessageToBackground(set);
        }

        public void GetTrackInfo()
        {
            ValueSet set = new ValueSet()
            {
                {"Command","TrackInfo"}
            };
            BackgroundMediaPlayer.SendMessageToBackground(set);
        }
        public void IsStopped()
        {
            ValueSet set = new ValueSet()
            {
                {"Command","IsStopped"}
            };
            BackgroundMediaPlayer.SendMessageToBackground(set);
        }

        public void Stop()
        {
            ValueSet set = new ValueSet()
            {
                {"Command","Stop"}
            };
            BackgroundMediaPlayer.SendMessageToBackground(set);
        }
        public async void PlayOffline(PodcastItem pod)
        {
            Debug.WriteLine("Play offline");
            StorageFile file = await getLocalFile(pod.Link);
            ValueSet set = new ValueSet()
            {
                {"Command","PlayOffline" },
                {"Thumb", pod.Immagine},
                {"Artist", "Calavera Cafè" },
                {"Title", pod.TitoloBG },
                {"Path", file.Path}
            };
            BackgroundMediaPlayer.SendMessageToBackground(set);
        }
        public void Pause()
        {
            ValueSet set = new ValueSet()
            {
                {"Command","Pause" }
            };
            BackgroundMediaPlayer.SendMessageToBackground(set);
        }
        public void Play()
        {
            ValueSet set = new ValueSet()
            {
                {"Command","Play" }
            };
            BackgroundMediaPlayer.SendMessageToBackground(set);
        }
        public void IsPlaying()
        {
            ValueSet set = new ValueSet()
            {
                {"Command","IsPlaying" }
            };
            BackgroundMediaPlayer.SendMessageToBackground(set);
        }
        public void IsPlayerLoaded()
        {
            ValueSet set = new ValueSet()
            {
                {"Command","IsTrackLoaded" }
            };
            BackgroundMediaPlayer.SendMessageToBackground(set);
        }
        public void GetPosition()
        {
            ValueSet set = new ValueSet()
            {
                {"Command","GetPosition" }
            };
            BackgroundMediaPlayer.SendMessageToBackground(set);
        }
    }
}
