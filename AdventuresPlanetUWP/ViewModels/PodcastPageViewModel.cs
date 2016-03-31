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
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace AdventuresPlanetUWP.ViewModels
{
    public class PodcastPageViewModel : Mvvm.ViewModelBase
    {
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (!await Settings.Instance.IsPodcastUpdated())
            {
                AggiornaPodcast();
            }
            BackgroundMediaPlayer.MessageReceivedFromBackground += MessageReceived;
            BackgroundMediaPlayer.Current.CurrentStateChanged += PlayerStateChanged;
            PodcastManager.Instance.IsPlayerLoaded();
            PlayerStateChanged(BackgroundMediaPlayer.Current, null);
            VerificaIsSalvabile();
            //return Task.CompletedTask;
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
            WindowWrapper.Current().Dispatcher.Dispatch(async () =>
            {
                switch (sender.CurrentState)
                {
                    case MediaPlayerState.Opening:
                        {
                            IsBuffering = true;
                            IsPlaying = false;
                            IsPlayerLoaded = true;
                            PodcastItem podcast = PodcastManager.Instance.CurrentItem;
                            if (podcast != null)
                            {
                                long position = Settings.Instance.PodcastPosition(podcast.Filename);
                                if (Settings.Instance.RicordaPosizionePodcast && position > 100)
                                {
                                    PodcastManager.Instance.Pause();
                                    MessageDialog msg = new MessageDialog("Vuoi riprendere dalla posizione salvata?");
                                    UICommand yes = new UICommand("Si", (s) =>
                                    {
                                        PodcastManager.Instance.SetPosition(position);
                                        PodcastManager.Instance.Play();
                                    },
                                    0);
                                    UICommand no = new UICommand("No", (s) => { PodcastManager.Instance.Play(); }, 1);
                                    msg.Commands.Add(yes);
                                    msg.Commands.Add(no);
                                    msg.CancelCommandIndex = 1;
                                    msg.DefaultCommandIndex = 0;
                                    await msg.ShowAsync();
                                }
                            }
                        }
                        break;
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
                VerificaIsSalvabile();
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
        private bool _isUpdating, _isPlaying, _isBuffering, _isPlayerLoaded, _isSaveable;
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
        public bool IsSaveable
        {
            get
            {
                return _isSaveable;
            }
            set
            {
                Set(ref _isSaveable, value);
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
        public void SalvaPosizionePodcast(object s = null, object e = null)
        {
            if(PodcastManager.Instance.CurrentItem != null)
            {
                long position = BackgroundMediaPlayer.Current.Position.Ticks;
                Settings.Instance.SavePodcastPosition(PodcastManager.Instance.CurrentItem.Filename, position);
            }
        }
        public async void GoToFacebook(object s, object e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://www.facebook.com/calaveracafepodcast/"));
        }
        public async void GoToTelegram(object s, object e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://telegram.me/calaveracafe"));
        }
        public async void InviaMail(object s, object e)
        {
            await Launcher.LaunchUriAsync(new Uri("mailto:calaveracafe@adventuresplanet.it"));
        }
        private void VerificaIsSalvabile()
        {
            if (Settings.Instance.RicordaPosizionePodcast && PodcastManager.Instance.CurrentItem!=null)
            {
                switch (BackgroundMediaPlayer.Current.CurrentState)
                {
                    case MediaPlayerState.Closed:
                    case MediaPlayerState.Opening:
                    case MediaPlayerState.Stopped:
                        IsSaveable = false;
                        break;
                    default:
                        IsSaveable = true;
                        break;
                }
            }
            if (!IsPlayerLoaded)
                IsSaveable = false;
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
