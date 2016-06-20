using AdventuresPlanetUWP.ViewModels;
using System.ComponentModel;
using System.Diagnostics;
using Template10.Common;
using Template10.Controls;
using Template10.Services.NavigationService;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System;
using Windows.UI.Xaml;
using AdventuresPlanetUWP.Classes;
using AdventuresPlanetUWP.Views.ContentDialogs;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace AdventuresPlanetUWP.Views
{
    // DOCS: https://github.com/Windows-XAML/Template10/wiki/Docs-%7C-SplitView
    public sealed partial class Shell : Page, INotifyPropertyChanged
    {
        public static Shell Instance { get; set; }
        public static HamburgerMenu HamburgerMenu { get { return Instance.MyHamburgerMenu; } }

        public Shell(INavigationService navigationService)
        {
            Instance = this;
            InitializeComponent();
            MyHamburgerMenu.NavigationService = navigationService;
            EnableAds();
        }

        private void HamburgerOpen(object sender, EventArgs e)
        {
            if (IsAdsEnabled && Window.Current.Bounds.Width<1200)
                AdsContainer.Visibility = Visibility.Collapsed;
        }

        private void HamburgerClosed(object sender, EventArgs e)
        {
            if (IsAdsEnabled)
                AdsContainer.Visibility = Visibility.Visible;
        }

        public bool IsBusy { get; set; } = false;
        public string BusyText { get; set; } = "Please wait...";
        public event PropertyChangedEventHandler PropertyChanged;

        public static void SetBusy(bool busy, string text = null)
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                if (busy)
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                else
                    BootStrapper.Current.UpdateShellBackButton();

                Instance.IsBusy = busy;
                Instance.BusyText = text;

                Instance.PropertyChanged?.Invoke(Instance, new PropertyChangedEventArgs(nameof(IsBusy)));
                Instance.PropertyChanged?.Invoke(Instance, new PropertyChangedEventArgs(nameof(BusyText)));
            });
        }
        public bool ShowMessage { get; set; } = false;
        public string MessageText { get; set; } = string.Empty;
        public void ShowMessagePopup(string message, bool error = false)
        {
            MessageText = message;
            if (!error)
                MessageContainer.Background = new SolidColorBrush(Colors.LimeGreen);
            else
                MessageContainer.Background = new SolidColorBrush(Colors.Red);
            ShowMessage = true;

            Instance.PropertyChanged?.Invoke(Instance, new PropertyChangedEventArgs(nameof(MessageText)));
            Instance.PropertyChanged?.Invoke(Instance, new PropertyChangedEventArgs(nameof(ShowMessage)));

            DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(1500) };
            timer.Tick += (s,e) =>
            {
                CloseMessagePopup();
                timer.Stop();
            };
            timer.Start();
        }

        public void CloseMessagePopup(object s = null, object e = null)
        {
            MessageText = string.Empty;
            ShowMessage = false;
            Instance.PropertyChanged?.Invoke(Instance, new PropertyChangedEventArgs(nameof(MessageText)));
            Instance.PropertyChanged?.Invoke(Instance, new PropertyChangedEventArgs(nameof(ShowMessage)));
        }
        private ResourceLoader resHoliday = ResourceLoader.GetForCurrentView("Holidays");
        private async void Auguri(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Tento di fare gli auguri");
            string message = resHoliday.GetString("auguri_feste");
            if (ChristmasTime.IsChristmas())
                message = resHoliday.GetString("auguri_natale");
            else if (ChristmasTime.IsCapodanno())
                message = resHoliday.GetString("auguri_anno_nuovo");
            else if (ChristmasTime.IsEpifania())
                message = resHoliday.GetString("auguri_epifania");

            await new MessageDialog(message, resHoliday.GetString("auguri_titolo")).ShowAsync();
        }
        public bool IsAdsEnabled { get; set; } = false;
        private async void EnableAds()
        {
            if (await Settings.Instance.IsAdsActive())
            {
                if (!IAPManager.Instance.IsProductActive(IAPCodes.REMOVE_ADS))
                {
                    FrameworkElement adsCont = FindName("AdsContainer") as FrameworkElement;
                    adsCont.Visibility = Visibility.Visible;
                    IsAdsEnabled = true;
                    Instance.PropertyChanged?.Invoke(Instance, new PropertyChangedEventArgs(nameof(IsAdsEnabled)));
                    return;
                }
            }
        }
        private async void RemoveAds(object sender, RoutedEventArgs e)
        {
            BuyRemoveAdsContentDialog dlg = new BuyRemoveAdsContentDialog();
            dlg.AtFinish = () =>
            {
                Debug.WriteLine("Running AtFinish");
                bool AdsRemoved = IAPManager.Instance.IsProductActive(IAPCodes.REMOVE_ADS);
                if (AdsRemoved)
                    CloseAds();
                Debug.WriteLine("AdsRemoved value = " + AdsRemoved);
            };
            await dlg.ShowAsync();
        }
        private void CloseAds()
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                if (AdsContainer != null)
                {
                    AdsContainer.Children.Clear();
                    AdsContainer.Visibility = Visibility.Collapsed;
                    AdsContainer = null;
                }
                IsAdsEnabled = false;
                Instance.PropertyChanged?.Invoke(Instance, new PropertyChangedEventArgs(nameof(IsAdsEnabled)));
                MyHamburgerMenu.PaneClosed -= HamburgerClosed;
                MyHamburgerMenu.PaneOpened -= HamburgerOpen;
            });
        }
    }
}

