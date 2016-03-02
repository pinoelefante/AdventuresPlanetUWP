using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Template10.Services.SerializationService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace AdventuresPlanetUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImageViewPage : Page
    {
        public ImageViewPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string url = SerializationService.Json.Deserialize<string>(e.Parameter.ToString());
            image.Source = new BitmapImage(new Uri(url));
        }
        private void zoomIn(object sender, RoutedEventArgs e)
        {
            if (scroller.ZoomFactor < scroller.MaxZoomFactor)
                scroller.ChangeView(scroller.HorizontalOffset, scroller.VerticalOffset, scroller.ZoomFactor + 0.25f);
        }

        private void zoomOut(object sender, RoutedEventArgs e)
        {
            if (scroller.ZoomFactor > scroller.MinZoomFactor)
                scroller.ChangeView(scroller.HorizontalOffset, scroller.VerticalOffset, scroller.ZoomFactor - 0.25f);
        }

        private void DoubleTapImage(object sender, DoubleTappedRoutedEventArgs e)
        {

            double x = e.GetPosition(sender as FrameworkElement).X;
            double y = e.GetPosition(sender as FrameworkElement).Y;
            scroller.ChangeView(scroller.HorizontalOffset + y, scroller.VerticalOffset + x, 1f);
        }
    }
}
