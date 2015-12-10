using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AdventuresPlanetUWP.Classes
{
    public class DownloadManager : IDisposable
    {
        private static DownloadManager _instance;
        public static DownloadManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DownloadManager();
                return _instance;
            }
        }
        private BackgroundDownloader downloader;
        private CancellationTokenSource cts;
        public List<DownloadItem> ListaDownload { get; set; }
        private DownloadManager()
        {
            downloader = new BackgroundDownloader();
            cts = new CancellationTokenSource();
            ListaDownload = new List<DownloadItem>();
        }
        public async void Init()
        {
            IReadOnlyList<DownloadOperation> downloads = null;
            try
            {
                downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Discovery error", ex);
                return;
            }
            if(downloads.Count > 0)
            {
                List<Task> tasks = new List<Task>();
                foreach (DownloadOperation download in downloads)
                {
                    //Log(String.Format(CultureInfo.CurrentCulture, "Discovered background download: {0}, Status: {1}", download.Guid, download.Progress.Status));

                    // Attach progress and completion handlers.
                    tasks.Add(HandleDownloadAsync(download));
                }

                // Don't await HandleDownloadAsync() in the foreach loop since we would attach to the second
                // download only when the first one completed; attach to the third download when the second one
                // completes etc. We want to attach to all downloads immediately.
                // If there are actions that need to be taken once downloads complete, await tasks here, outside
                // the loop.
                await Task.WhenAll(tasks);
            }
        }
        private void DownloadProgress(DownloadOperation download)
        {
            DownloadItem downItem = GetDownload(download.RequestedUri.AbsoluteUri);
            int progress = (int)(100 * ((double)download.Progress.BytesReceived / (double)download.Progress.TotalBytesToReceive));
            string textProgress = null;
            switch (download.Progress.Status)
            {
                case BackgroundTransferStatus.Running:
                    {
                        textProgress = "Download in corso...";
                        break;
                    }
                case BackgroundTransferStatus.PausedByApplication:
                    {
                        textProgress = "Download in pausa";
                        break;
                    }
                case BackgroundTransferStatus.PausedCostedNetwork:
                    {
                        textProgress = "Download messo in pausa per l'utilizzo di connessione a consumo";
                        break;
                    }
                case BackgroundTransferStatus.PausedNoNetwork:
                    {
                        textProgress = "Controlla la connessione ad internet";
                        break;
                    }
                case BackgroundTransferStatus.Error:
                    {
                        textProgress = "Si è verificato un errore durante il download";
                        break;
                    }
            }
            if (progress >= 100)
            {
                textProgress = "Download completato";
            }
            if (downItem?.TextProgress != null)
                downItem.TextProgress.Text = $"{textProgress} - {progress}%";
        }
        private async Task HandleDownloadAsync(DownloadOperation download, TextBlock tb = null)
        {
            try
            {
                ListaDownload.Add(new DownloadItem(download, tb));

                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                await download.StartAsync().AsTask(cts.Token, progressCallback);
                ResponseInformation response = download.GetResponseInformation();
            }
            catch (TaskCanceledException)
            {
                //LogStatus("Canceled: " + download.Guid, NotifyType.StatusMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                /*
                if (!IsExceptionHandled("Execution error", ex, download))
                {
                    throw;
                }
                */
            }
            finally
            {
                ListaDownload.Remove(GetDownload(download.RequestedUri.AbsoluteUri));
            }
        }
        public async void DownloadPodcast(string url, TextBlock tb)
        {
            Debug.WriteLine("Creo download di " + url);
            StorageFolder music = KnownFolders.MusicLibrary;
            StorageFolder podcastDir = await music.CreateFolderAsync("Podcast", CreationCollisionOption.OpenIfExists);
            
            StorageFile destFile = await podcastDir.CreateFileAsync(url.Substring(url.LastIndexOf("/")+1), CreationCollisionOption.ReplaceExisting);
            DownloadOperation download = downloader.CreateDownload(new Uri(url), destFile);
            Debug.WriteLine("Download avviato di " + url);
            download.Priority = BackgroundTransferPriority.Default;
            await HandleDownloadAsync(download, tb);
        }
        public DownloadItem GetDownload(string url)
        {
            foreach(DownloadItem d in ListaDownload)
            {
                if (d.Download.RequestedUri.AbsoluteUri.Equals(url))
                    return d;
            }
            return null;
        }
        public void Dispose()
        {
            if (cts != null)
            {
                cts.Dispose();
                cts = null;
            }
        }
    }
    public class DownloadItem
    {
        private DownloadOperation download;
        private TextBlock progress;

        public DownloadItem(DownloadOperation x)
        {
            Download = x;
        }
        public DownloadItem(DownloadOperation x, TextBlock p)
        {
            Download = x;
            TextProgress = p;
        }
        public DownloadOperation Download {
            get
            {
                return download;
            }
            set
            {
                download = value;
            }
        }
        public TextBlock TextProgress
        {
            get
            {
                return progress;
            }
            set
            {
                progress = value;
            }
        }
    }
}
