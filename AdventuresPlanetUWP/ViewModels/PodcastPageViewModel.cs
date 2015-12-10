using AdventuresPlanetUWP.Classes;
using AdventuresPlanetUWP.Classes.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace AdventuresPlanetUWP.ViewModels
{
    public class PodcastPageViewModel : Mvvm.ViewModelBase
    {
        public PodcastPageViewModel()
        {
            
        }
        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            Debug.WriteLine("podcast = " + ListPodcast.Count);
        }
        public async void AggiornaPodcast(object s, object e)
        {
            IsUpdatingPodcast = true;
            await AdventuresPlanetManager.Instance.aggiornaPodcast();
            IsUpdatingPodcast = false;
        }
        public ObservableCollection<PodcastItem> ListPodcast { get; } = AdventuresPlanetManager.Instance.ListaPodcast;
        private bool _isUpdating;
        public bool IsUpdatingPodcast
        {
            get
            {
                return _isUpdating;
            }
            set
            {
                Set<bool>(ref _isUpdating, value);
            }
        }
    }
}
