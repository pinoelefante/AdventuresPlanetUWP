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
        private ResourceLoader resHoliday = ResourceLoader.GetForCurrentView("Holidays");
        private async void Auguri(object sender, Windows.UI.Xaml.RoutedEventArgs e)
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
        private async void EnableAds()
        {
            if (await Settings.Instance.IsAdsActive())
            {
                if (!IAPManager.Instance.IsProductActive(IAPCodes.REMOVE_ADS))
                {
                    FrameworkElement adsCont = FindName("AdsContainer") as FrameworkElement;
                    adsCont.Visibility = Visibility.Visible;
                }
            }
        }
        private async void RemoveAds(object sender, Windows.UI.Xaml.RoutedEventArgs e)
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
            if (AdsContainer != null)
            {
                AdsContainer.Children.Clear();
                AdsContainer.Visibility = Visibility.Collapsed;
                AdsContainer = null;
            }
        }
    }
}

