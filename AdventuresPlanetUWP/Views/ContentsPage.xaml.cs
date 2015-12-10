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
    }
}
