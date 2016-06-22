using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace AdventuresPlanetUWP.Mvvm
{
    // DOCS: https://github.com/Windows-XAML/Template10/wiki/Docs-%7C-MVVM
    public abstract class ViewModelBase : Template10.Mvvm.ViewModelBase
    {
        public ViewModelBase()
        {
            Views.Shell.Instance.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName.Equals("IsAdsEnabled"))
                {
                    RaisePropertyChanged(nameof(MarginFrame));
                }
            };
        }
        // the only thing that matters here is Template10.Services.NavigationService.INavagable
        private Thickness _margins;
        public Thickness MarginFrame {
            get
            {
                if (Views.Shell.Instance.IsAdsEnabled)
                    return new Thickness(0, 0, 0, 80);
                else
                    return new Thickness(0);
            }
        }
        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            RaisePropertyChanged(nameof(MarginFrame));
            return base.OnNavigatedToAsync(parameter, mode, state);
        }
    }
}
