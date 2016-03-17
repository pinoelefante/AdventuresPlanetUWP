using AdventuresPlanetUWP.Classes;
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
        private long lastSave = 0;
        private void lay(object sender, object e)
        {
            ScrollViewer scroll = VisualTreeHelperExtensions.GetFirstDescendantOfType<ScrollViewer>(containerData);
            long currTime = Settings.getUnixTimeStamp();
            if ((currTime - lastSave) < 2)
                return;
            bool save = (VM.IsSoluzione && Settings.Instance.RicordaPosizioneSoluzioni) || (VM.IsRecensione && Settings.Instance.RicordaPosizioneRecensioni);
            if(scroll != null && VM.IsLoaded && save && !VM.Item.isTemporary && !VM.Item.isVideo)
            {
                //Debug.WriteLine("Salvo la posizione");
                double vOffset = scroll.VerticalOffset;
                int index = 0;
                for(double cOff = 0; index<containerData.Items.Count; index++)
                {
                    FrameworkElement elem = containerData.Items[index] as FrameworkElement;
                    cOff += elem.ActualHeight;
                    if (cOff >= vOffset)
                        break;
                }
                VM.SalvaPosizione(index);
                lastSave = currTime;
            }
        }
        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as FrameworkElement;
            var index = containerData.Items.IndexOf(item);
            VM.SalvaPosizione(index);
        }
    }
}
