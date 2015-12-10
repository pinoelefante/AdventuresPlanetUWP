using AdventuresPlanetUWP.Classes;
using AdventuresPlanetUWP.Classes.Data;
using AdventuresPlanetUWP.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace AdventuresPlanetUWP.Views.UserControls
{
    public sealed partial class UCPodcast : UserControl, IDisposable
    {
        public PodcastItem Podcast => this.DataContext as PodcastItem;
        public UCPodcast()
        {
            this.InitializeComponent();
            
            Loaded += (s,e) =>
            {
                getStatusDownload(Podcast);
            };
        }
        public void Dispose()
        {
            
        }

        private void scaricaPodcast(object sender, TappedRoutedEventArgs e)
        {
            PodcastItem item = (sender as FrameworkElement).DataContext as PodcastItem;
            DownloadManager.Instance.DownloadPodcast(item.Link, progressDownload);
            progressDownload.Visibility = Visibility.Visible;
            getStatusDownload(item);
        }
        DownloadItem download;
        private void getStatusDownload(PodcastItem item)
        {
            download = DownloadManager.Instance.GetDownload(item?.Link);
            if(download != null)
            {
                progressDownload.Visibility = Visibility.Visible;
                download.TextProgress = progressDownload;
            }
        }
        private async void podcastDescrizione(object sender, TappedRoutedEventArgs e)
        {
            await new MessageDialog(Podcast.Descrizione, Podcast.TitoloBG).ShowAsync();
        }

        private void onTapPodcast(object sender, TappedRoutedEventArgs e)
        {
            PodcastItem pod = (sender as FrameworkElement).DataContext as PodcastItem;
            PodcastManager.Instance.Play(pod);
        }

        private void share(object sender, TappedRoutedEventArgs e)
        {

        }

        private void OpenFlyout(object sender, object e)
        {
            Flyout.ShowAttachedFlyout(sender as FrameworkElement);
        }
    }
}
