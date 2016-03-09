using AdventuresPlanetUWP.Classes;
using AdventuresPlanetUWP.Classes.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace AdventuresPlanetUWP.ViewModels
{
    public class PodcastPageViewModel : Mvvm.ViewModelBase
    {
        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (!Settings.Instance.IsPodcastUpdated)
            {
                AggiornaPodcast();
            }
            BackgroundMediaPlayer.MessageReceivedFromBackground += MessageReceived;
            BackgroundMediaPlayer.Current.CurrentStateChanged += PlayerStateChanged;
            PodcastManager.Instance.IsPlayerLoaded();
            return Task.CompletedTask;
        }
        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            BackgroundMediaPlayer.MessageReceivedFromBackground -= MessageReceived;
            BackgroundMediaPlayer.Current.CurrentStateChanged -= PlayerStateChanged;
            StopGetPositionRequests();
            return Task.CompletedTask;
        }
        private void PlayerStateChanged(MediaPlayer sender, object args)
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                switch (sender.CurrentState)
                {
                    case MediaPlayerState.Buffering:
                        IsBuffering = true;
                        IsPlaying = false;
                        IsPlayerLoaded = true;
                        break;
                    case MediaPlayerState.Paused:
                        IsBuffering = false;
                        IsPlaying = false;
                        IsPlayerLoaded = true;
                        break;
                    case MediaPlayerState.Playing:
                        IsBuffering = false;
                        IsPlaying = true;
                        PodcastManager.Instance.IsStopped(); //verifica che il file riprodotto non sia stop.mp3
                        AvviaGetPositionRequests();
                        break;
                    default:
                        IsBuffering = false;
                        IsPlaying = false;
                        if (dt != null)
                            dt.Stop();
                        IsPlayerLoaded = false;
                        break;
                }
                PodcastManager.Instance.IsStopped();
                PodcastManager.Instance.GetTrackInfo();
            });
        }
        private void MessageReceived(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                switch (e.Data["Command"].ToString())
                {
                    case "IsPlaying":
                        IsPlaying = (bool)e.Data["Status"];
                        if (IsPlaying)
                        {
                            PodcastManager.Instance.GetTrackInfo();
                            AvviaGetPositionRequests();
                        }
                        break;
                    case "GetPosition":
                        Durata = (long)e.Data["Durata"];
                        CurrentPosition = (long)e.Data["Position"];
                        break;
                    case "TrackInfo":
                        TitoloPodcast = e.Data["Title"].ToString();
                        break;
                    case "IsTrackLoaded":
                        IsPlayerLoaded = (bool)e.Data["Status"];
                        if(IsPlayerLoaded)
                            PodcastManager.Instance.IsPlaying();
                        break;
                    case "IsStopped":
                        IsPlayerLoaded = !(bool)e.Data["Status"];
                        break;
                }
            });
        }
        private DispatcherTimer dt;
        private void AvviaGetPositionRequests()
        {
            if (dt == null)
            {
                dt = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
                dt.Tick += (s, e) =>
                {
                    PodcastManager.Instance.GetPosition();
                };
            }
            if (!dt.IsEnabled)
                dt.Start();
        }
        private void StopGetPositionRequests()
        {
            if (dt != null)
            {
                dt.Stop();
            }
        }
        public async void AggiornaPodcast(object s = null, object e = null)
        {
            IsUpdatingPodcast = true;
            await AdventuresPlanetManager.Instance.aggiornaPodcast();
            IsUpdatingPodcast = false;
        }
        public void Play(object s, object e)
        {
            PodcastManager.Instance.Play();
        }
        public void Pause(object s, object e)
        {
            PodcastManager.Instance.Pause();
        }
        public void Stop(object s, object e)
        {
            PodcastManager.Instance.Stop();
            IsPlayerLoaded = false;
        }
        public ObservableCollection<PodcastItem> ListPodcast { get; } = AdventuresPlanetManager.Instance.ListaPodcast;
        private bool _isUpdating, _isPlaying, _isBuffering, _isPlayerLoaded;
        private string _title = string.Empty, _durataT = string.Empty, _curPosT = string.Empty;
        private long _durata = 1, _currPos = 0;
        public bool IsUpdatingPodcast
        {
            get
            {
                return _isUpdating;
            }
            set
            {
                Set<bool>(ref _isUpdating, value);
            }
        }
        public bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
            set
            {
                Set(ref _isPlaying, value);
            }
        }
        public bool IsBuffering
        {
            get
            {
                return _isBuffering;
            }
            set
            {
                Set(ref _isBuffering, value);
            }
        }
        public bool IsPlayerLoaded
        {
            get
            {
                return _isPlayerLoaded;
            }
            set
            {
                Set(ref _isPlayerLoaded, value);
            }
        }
        public string TitoloPodcast
        {
            get
            {
                return _title;
            }
            set
            {
                Set(ref _title, value);
            }
        }
        public long Durata
        {
            get
            {
                return _durata;
            }
            set
            {
                Set(ref _durata, value);
                TimeSpan dur = TimeSpan.FromTicks(_durata);
                DurataText = $"{dur.Hours}:{dur.Minutes.ToString("D2")}:{dur.Seconds.ToString("D2")}";
            }
        }
        public long CurrentPosition
        {
            get
            {
                return _currPos;
            }
            set
            {
                Set(ref _currPos, value);
                TimeSpan dur = TimeSpan.FromTicks(_currPos);
                CurrentPositionText = $"{dur.Hours}:{dur.Minutes.ToString("D2")}:{dur.Seconds.ToString("D2")}";
            }
        }
        public string DurataText
        {
            get
            {
                return _durataT;
            }
            set
            {
                Set(ref _durataT, value);
            }
        }
        public string CurrentPositionText
        {
            get
            {
                return _curPosT;
            }
            set
            {
                Set(ref _curPosT, value);
            }
        }
    }
}
