using AdventuresPlanetUWP.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace AdventuresPlanetUWP.Views.UserControls
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UCMediaPlayer : UserControl, INotifyPropertyChanged
    {
        private bool _loaded = false; 
        public UCMediaPlayer()
        {
            ticks_mediaLoaded = 0;
            this.InitializeComponent();
            this.Loaded += (s, e) =>
            {
                Debug.WriteLine("VideoPlayerManager = " + VideoPlayerManager.Instance.ListVideo.Count);
                checkVideoManager();
                MediaTransportControls ptc = new MediaTransportControls();
                player.TransportControls.IsStopButtonVisible = true;
                player.TransportControls.IsStopEnabled = true;
                player.TransportControls.IsZoomButtonVisible = false;
                player.TransportControls.IsZoomEnabled = false;
                player.TransportControls.IsCompact = true;
            };
        }
        private DispatcherTimer mediaLoadedDt;
        private int ticks_mediaLoaded = 0;
        private void checkVideoManager()
        {
            if (mediaLoadedDt == null)
            {
                mediaLoadedDt = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) };
                mediaLoadedDt.Tick += (s, e1) =>
                {
                    if (VideoPlayerManager.Instance.ItemLoaded && VideoPlayerManager.Instance.IsPlayable())
                    {
                        LoadFirst();
                        mediaLoadedDt.Stop();
                    }
                    else if (VideoPlayerManager.Instance.ItemLoaded)
                    {
                        if (ToLoad)
                        {
                            //Mostrare errore di caricamento video
                            MessageDialog msg = new MessageDialog("Errore durante il caricamento del video");
                            msg.ShowAsync();
                        }
                        mediaLoadedDt.Stop();
                    }
                    ticks_mediaLoaded++;
                    if (ticks_mediaLoaded > 20) // 20 * 0.5 secondi
                        mediaLoadedDt.Stop();
                };
            }
            if(!mediaLoadedDt.IsEnabled)
                mediaLoadedDt.Start();
        }
        private void LoadFirst()
        {
            if (VideoPlayerManager.Instance.IsPlayable())
            {
                CurrentItem = VideoPlayerManager.Instance.CurrentItem();
                Debug.WriteLine($"CurrentItem = Uri={CurrentItem.Uri}, Itag={CurrentItem.Itag}, VideoQuality={CurrentItem.VideoQuality}");
                player.Source = CurrentItem.Uri;
                player.Play();
                _loaded = true;
            }
            else
            {
                MessageDialog msg = new MessageDialog("Non ci sono video da riprodurre");
                msg.ShowAsync();
            }
        } 
        private YouTubeUri _current;
        public YouTubeUri CurrentItem
        {
            get
            {
                return _current;
            }
            set
            {
                _current = value;
                NotifyProperty();
            }
        }
        private void StopVideo(object sender, RoutedEventArgs e)
        {
            player.Stop();
            player.Position = player.NaturalDuration.TimeSpan;
            player.Source = null;
            _loaded = false;
        }

        private void PrevVideo(object sender, RoutedEventArgs e)
        {
            StopVideo(null, null);
            CurrentItem = VideoPlayerManager.Instance.Prev();
            player.Play();
        }

        private void NextVideo(object sender, RoutedEventArgs e)
        {
            StopVideo(null, null);
            CurrentItem = VideoPlayerManager.Instance.Next();
            player.Play();
        }
        private void MediaPlayerStateChanged(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Player State = " + player.CurrentState.ToString());
            switch (player.CurrentState)
            {
                case MediaElementState.Opening:
                case MediaElementState.Buffering:
                    IsBuffering = true;
                    break;
                case MediaElementState.Playing:
                case MediaElementState.Closed:
                case MediaElementState.Stopped:
                case MediaElementState.Paused:
                    IsBuffering = false;
                    break;
                default:
                    IsBuffering = false;
                    break;
            }
        }
        private bool _isBuffering;
        public bool IsBuffering
        {
            get
            {
                return _isBuffering;
            }
            set
            {
                _isBuffering = value;
                NotifyProperty();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyProperty([CallerMemberName]string p = null)
        {
            if (PropertyChanged != null && p!=null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        public static readonly DependencyProperty ToLoadProperty = DependencyProperty.Register("ToLoad", typeof(Boolean), typeof(UCMediaPlayer), null);
        public bool ToLoad { get { return (bool)GetValue(ToLoadProperty); } set { SetValue(ToLoadProperty, (bool)value); } }
    }
}
