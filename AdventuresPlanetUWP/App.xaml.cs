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
using System.Globalization;
using Windows.ApplicationModel.Resources;
using Template10.Common;

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
            this.UnhandledException += OnUnhandledException;
            #endregion
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine(e.GetType());
            Debug.WriteLine(e.Message +" \nSource = "+ e.Exception.Source);
            e.Handled = true;
            
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
                AdventuresPlanetManager.Instance.Load();
                NavigationService.Navigate(typeof(Views.NewsPage));
            }
        }
        
        private static DisplayRequest KSARequest;
        private static int KSACount = 0;
        internal static void KeepScreenOn_Release()
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
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
            });
            
        }

        internal static void KeepScreenOn()
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
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
            });
            
        }

        internal static bool IsInternetConnected()
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            bool internet = (connections != null) && (connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
            return internet;
        }
    }
}

