using System;
using Windows.UI.Xaml;
using System.Threading.Tasks;
using AdventuresPlanetUWP.Services.SettingsServices;
using Windows.ApplicationModel.Activation;
using AdventuresPlanetUWP.Classes;
using Windows.Networking.Connectivity;
using Windows.System.Display;
using System.Diagnostics;
using Windows.UI.Popups;
using Windows.ApplicationModel.Store;
using Windows.System;

namespace AdventuresPlanetUWP
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki

    sealed partial class App : Template10.Common.BootStrapper
    {
        ISettingsService _settings;

        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            InitializeComponent();
            SplashFactory = (e) => new Views.Splash(e);

            #region App settings

            _settings = SettingsService.Instance;
            //RequestedTheme = _settings.AppTheme;
            CacheMaxDuration = _settings.CacheMaxDuration;
            ShowShellBackButton = _settings.UseShellBackButton;


            #endregion

            #region customInit
            // Settings.Instance.reset();
            DatabaseSystem.Instance.Connect();
            PodcastManager.Instance.Init();
            #endregion
        }

        // runs even if restored from state
        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            // setup hamburger shell
            var nav = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);
            Window.Current.Content = new Views.Shell(nav);
            await Task.Yield();
        }

        // runs only when not restored from state
        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            // perform long-running load
            await Task.Delay(0);

            if (Settings.Instance.FirstStart)
                NavigationService.Navigate(typeof(Views.FirstStartPage));
            else
            {
                NavigationService.Navigate(typeof(Views.NewsPage));
                mostraVotaApplicazione();
            }
        }
        private async void mostraVotaApplicazione()
        {
            Debug.WriteLine("Numero avvio = " + Settings.Instance.NumeroAvvii);
            if (Settings.Instance.NumeroAvvii % 5 == 0 && !Settings.Instance.Votato)
            {
                MessageDialog msg = new MessageDialog("Dice: 'Chiedimi di votare.'\nNon mi va che la gente mi chieda sempre di votare", "Aiutaci!");
                UICommand si = new UICommand("Vota") { Id = 0 };
                si.Invoked = async (x) =>
                {
                    var uri = new Uri(string.Format("ms-windows-store:reviewapp?appid={0}", CurrentApp.AppId));
                    await Launcher.LaunchUriAsync(uri);
                    Settings.Instance.Votato = true;
                };
                UICommand no = new UICommand("Chiudi") { Id = 1 };
                no.Invoked = (x) => { };
                msg.Commands.Add(si);
                msg.Commands.Add(no);
                msg.CancelCommandIndex = 1;
                msg.DefaultCommandIndex = 0;

                await msg.ShowAsync();
            }
        }
        private static DisplayRequest KSARequest;
        private static int KSACount = 0;
        internal static void KeepScreenOn_Release()
        {
            if (KSACount > 0)
                KSACount--;
            if (KSACount == 0 && KSARequest != null)
                try
                {
                    KSARequest.RequestRelease();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
        }

        internal static void KeepScreenOn()
        {
            try
            {
                if (KSARequest == null)
                {
                    KSARequest = new DisplayRequest();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }

            if (KSARequest != null)
            {
                try
                {
                    KSARequest.RequestActive();
                    KSACount++;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex.Message);
                }
            }
        }

        internal static bool IsInternetConnected()
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            bool internet = (connections != null) && (connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
            return internet;
        }
    }
}

