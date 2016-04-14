using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.System;
using Windows.UI.Xaml.Navigation;

namespace AdventuresPlanetUWP.ViewModels
{
    public class InfoPageViewModel : Mvvm.ViewModelBase
    {
        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            return base.OnNavigatedToAsync(parameter, mode, state);
        }
        public string Versione
        {
            get
            {
                var myPackage = Windows.ApplicationModel.Package.Current;
                var version = myPackage.Id.Version;

                var appVersion = version.Major + "." +
                                 version.Minor + "." +
                                 version.Build;
                return appVersion;
            }
        }
        public async void ContattaSviluppatore(object s, object e)
        {
            await Launcher.LaunchUriAsync(new Uri($"mailto:pinoelefante@hotmail.it?subject=[Feedback] Adventure's Planet ver. {Versione}"));
        }
        public async void Vota(object s, object e)
        {
            var uri = new Uri(string.Format("ms-windows-store:reviewapp?appid={0}", CurrentApp.AppId));
            await Launcher.LaunchUriAsync(uri);
        }
        public async void VaiAlSitoWeb(object s, object e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://www.adventuresplanet.it"));
        }
    }
}
