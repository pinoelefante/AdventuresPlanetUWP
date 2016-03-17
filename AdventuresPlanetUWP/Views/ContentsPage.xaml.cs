using AdventuresPlanetUWP.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace AdventuresPlanetUWP.Views
{
    public sealed partial class ContentsPage : Page
    {
        public ContentsPage()
        {
            this.InitializeComponent();
        }
        public ContentsPageViewModel VM => this.DataContext as ContentsPageViewModel;

        private void ShowTitle(object sender, TappedRoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout(sender as FrameworkElement);
        }

        private void GoToIndex(object sender, object e)
        {
            //Debug.WriteLine(sender.GetType());
            IndiceItem indice = (sender as FrameworkElement).DataContext as IndiceItem;
            VM.GoToIndex(containerData, indice);
            //semZoom.IsZoomedInViewActive = true;
        }

        private void apriIndice(object sender, RoutedEventArgs e)
        {
            semZoom.IsZoomedInViewActive = false;
        }

        private void ListView_LayoutUpdated(object sender, object e)
        {
            Debug.WriteLine("Layout Updated");
        }

        private void ListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            Debug.WriteLine("Container content changing");
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Debug.WriteLine("Size Changed");
        }
    }
}
