using AdventuresPlanetUWP.Classes;
using AdventuresPlanetUWP.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace AdventuresPlanetUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ViewSoluzione : Page
    {
        public ViewSoluzione()
        {
            this.InitializeComponent();
        }
        public ContentsPageViewModel VM => this.DataContext as ContentsPageViewModel;
        private void GoToIndex(object sender, object e)
        {
            IndiceItem indice = (sender as FrameworkElement).DataContext as IndiceItem;
            VM.GoToIndex(containerData, indice);
        }

        private void apriIndice(object sender, RoutedEventArgs e)
        {
            semZoom.IsZoomedInViewActive = false;
        }
        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as FrameworkElement;
            var index = containerData.Items.IndexOf(item);
            VM.SalvaPosizione(index);
        }
        private ScrollViewer scroll;
        private void OnScrollLoaded(object sender, RoutedEventArgs e)
        {
            scroll = VisualTreeHelperExtensions.GetFirstDescendantOfType<ScrollViewer>(containerData);
            LoadPosition();
            scroll.ViewChanged -= savePosition;
            scroll.ViewChanged += savePosition;
        }

        private void savePosition(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!VM.ItemLoaded)
                return;
            if (!_positionLoaded)
                LoadPosition();
            long currTime = Settings.getUnixTimeStamp();
            bool save = (VM.IsSoluzione && Settings.Instance.RicordaPosizioneSoluzioni);
            if (scroll != null && save && VM.Item.IsSaveable() && _positionLoaded)
            {
                double vOffset = scroll.VerticalOffset;
                int index = 0;
                for (double cOff = 0; index < containerData.Items.Count; index++)
                {
                    FrameworkElement elem = containerData.Items[index] as FrameworkElement;
                    cOff += elem.ActualHeight;
                    if (cOff >= vOffset)
                        break;
                }
                System.Diagnostics.Debug.WriteLine("Index = " + index);
                VM.SalvaPosizione(index);
            }
        }
        private bool _positionLoaded = false;
        private void LoadPosition()
        {
            if (Settings.Instance.RicordaPosizioneSoluzioni && VM.ItemLoaded && VM.Item.IsSaveable())
            {
                int indexPos = Settings.Instance.SoluzionePosition(VM.Item.Id);
                if (indexPos < containerData.Items?.Count)
                    containerData.ScrollIntoView(containerData.Items.ElementAt(indexPos), ScrollIntoViewAlignment.Leading);
                _positionLoaded = true;
            }
        }
    }
}
