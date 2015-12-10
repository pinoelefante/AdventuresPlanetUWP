using AdventuresPlanetUWP.Classes.Data;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace AdventuresPlanetUWP.Views.UserControls
{
    public sealed partial class UCNews : UserControl
    {
        private News news => this.DataContext as News;
        public UCNews()
        {
            this.InitializeComponent();
        }
    }
}
