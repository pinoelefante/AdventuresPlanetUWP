using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PodcastPlayer
{
    public sealed class MediaPlayerPod : IBackgroundTask
    {
        private BackgroundTaskDeferral deferral;
        private SystemMediaTransportControls systemmediatransportcontrol;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // Initialize SMTC object to talk with UniversalVolumeControl (UVC)
            // Note that, this is intended to run after app is paused and hence all the logic must be written to run in background process
            systemmediatransportcontrol = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
            systemmediatransportcontrol.ButtonPressed += SystemControlsButtonPressed;
            systemmediatransportcontrol.IsEnabled = true;
            systemmediatransportcontrol.IsPauseEnabled = true;
            systemmediatransportcontrol.IsStopEnabled = true;
            systemmediatransportcontrol.IsPlayEnabled = true;

            // Add handlers for MediaPlayer
            BackgroundMediaPlayer.Current.CurrentStateChanged -= BackgroundMediaPlayerCurrentStateChanged;
            BackgroundMediaPlayer.Current.CurrentStateChanged += BackgroundMediaPlayerCurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromForeground -= BackgroundMediaPlayerOnMessageReceivedFromForeground;
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayerOnMessageReceivedFromForeground;
            deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskIstance_Cancelled;
        }

        private void TaskIstance_Cancelled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            deferral.Complete();
        }
        private string PlayingTitle = string.Empty;
        private async void BackgroundMediaPlayerOnMessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            // Update the UVC text
            switch (e.Data["Command"]?.ToString())
            {
                case "Init":
                    break;
                case "IsPlaying":
                    bool isPlaying = BackgroundMediaPlayer.Current.Source != null && BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing;
                    BackgroundMediaPlayer.SendMessageToForeground(new ValueSet()
                    {
                        { "Command", "IsPlaying" },
                        { "Status", isPlaying }
                    });
                    break;
                case "IsStopped":
                    BackgroundMediaPlayer.SendMessageToForeground(new ValueSet()
                    {
                        { "Command", "IsStopped" },
                        { "Status", IsStopped }
                    });
                    break;
                case "GetPosition":
                    long durata = BackgroundMediaPlayer.Current.NaturalDuration.Ticks;
                    long position = BackgroundMediaPlayer.Current.Position.Ticks;
                    BackgroundMediaPlayer.SendMessageToForeground(new ValueSet()
                    {
                        { "Command", "GetPosition" },
                        { "Durata", durata },
                        { "Position", position }
                    });
                    break;
                case "SetPosition":
                    long newPosition = (long)e.Data["NewPosition"];
                    BackgroundMediaPlayer.Current.Position = new TimeSpan(newPosition);
                    break;
                case "TrackInfo":
                    BackgroundMediaPlayer.SendMessageToForeground(new ValueSet()
                    {
                        { "Command", "TrackInfo" },
                        { "Title", PlayingTitle }
                    });
                    break;
                case "IsTrackLoaded":
                    {
                        bool isLoaded = IsStopped == false;
                        BackgroundMediaPlayer.SendMessageToForeground(new ValueSet()
                        {
                            { "Command", "IsTrackLoaded" },
                            { "Status", isLoaded }
                        });
                    }
                    break;
                case "Stop":
                    Stop();
                    break;
                case "PlayOffline":
                    {
                        IsStopped = false;
                        string path = e.Data["Path"].ToString();
                        string imageOF = e.Data["Thumb"].ToString();
                        StorageFile fileOff = await StorageFile.GetFileFromPathAsync(path);
                        BackgroundMediaPlayer.Current.SetFileSource(fileOff);
                        BackgroundMediaPlayer.Current.Play();
                        systemmediatransportcontrol.DisplayUpdater.Type = MediaPlaybackType.Music;
                        PlayingTitle = e.Data["Title"].ToString();
                        systemmediatransportcontrol.DisplayUpdater.MusicProperties.Title = e.Data["Title"].ToString();
                        systemmediatransportcontrol.DisplayUpdater.MusicProperties.Artist = e.Data["Artist"].ToString();
                        systemmediatransportcontrol.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(imageOF));
                        systemmediatransportcontrol.DisplayUpdater.Update();
                    }
                    break;
                case "PlayOnline":
                    {
                        IsStopped = false;
                        string Url = e.Data["Url"].ToString();
                        string imageO = e.Data["Thumb"].ToString();
                        BackgroundMediaPlayer.Current.SetUriSource(new Uri(Url));
                        BackgroundMediaPlayer.Current.Play();
                        systemmediatransportcontrol.DisplayUpdater.Type = MediaPlaybackType.Music;
                        PlayingTitle = e.Data["Title"].ToString();
                        systemmediatransportcontrol.DisplayUpdater.MusicProperties.Title = e.Data["Title"].ToString();
                        systemmediatransportcontrol.DisplayUpdater.MusicProperties.Artist = e.Data["Artist"].ToString();
                        systemmediatransportcontrol.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(imageO));
                        systemmediatransportcontrol.DisplayUpdater.Update();
                    }
                    break;
                case "Play":
                    IsStopped = false;
                    BackgroundMediaPlayer.Current.Play();
                    break;
                case "Pause":
                    IsStopped = false;
                    BackgroundMediaPlayer.Current.Pause();
                    break;
            }
        }
        private bool IsStopped = true;
        private async void Stop()
        {
            IsStopped = true;
            BackgroundMediaPlayer.Current.SystemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
            BackgroundMediaPlayer.Current.Position = BackgroundMediaPlayer.Current.NaturalDuration;
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///stop.mp3"));
            BackgroundMediaPlayer.Current.SetFileSource(file);
            PlayingTitle = string.Empty;
            systemmediatransportcontrol.DisplayUpdater.ClearAll();
            systemmediatransportcontrol.DisplayUpdater.Update();
        }
        private void BackgroundMediaPlayerCurrentStateChanged(MediaPlayer sender, object args)
        {
            // Update UVC button state
            if (sender.CurrentState == MediaPlayerState.Playing)
            {
                systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Playing;
            }
            else if (sender.CurrentState == MediaPlayerState.Paused)
            {
                systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Paused;
            }
        }

        private void SystemControlsButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            // Pass UVC commands on to the Background player
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    BackgroundMediaPlayer.Current.Play();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    BackgroundMediaPlayer.Current.Pause();
                    if(BackgroundMediaPlayer.Current.Position == BackgroundMediaPlayer.Current.NaturalDuration)
                    {
                        IsStopped = true;
                        BackgroundMediaPlayer.SendMessageToForeground(new ValueSet()
                        {
                            { "Command", "IsStopped" },
                            { "Status", IsStopped }
                        });
                        systemmediatransportcontrol.DisplayUpdater.ClearAll();
                        systemmediatransportcontrol.DisplayUpdater.Update();
                    }
                    break;
                case SystemMediaTransportControlsButton.Stop:
                    break;
            }
        }
    }
}
