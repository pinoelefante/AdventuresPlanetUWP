using AdventuresPlanetUWP.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;

namespace AdventuresPlanetUWP.ViewModels
{
    public class FirstStartViewModel : Mvvm.ViewModelBase, IDisposable
    {
        public FirstStartViewModel()
        {
        }
        private bool _rec,_sol,_pod;
        public bool IsRecensioniLoaded
        {
            get
            {
                return _rec;
            }
            set
            {
                Set<bool>(ref _rec, value);
            }
        }
        public bool IsSoluzioniLoaded
        {
            get
            {
                return _sol;
            }
            set
            {
                Set<bool>(ref _sol, value);
            }
        }
        public bool IsPodcastLoaded
        {
            get
            {
                return _pod;
            }
            set
            {
                Set<bool>(ref _pod, value);
            }
        }
        private List<Task> listTask;
        public async Task CheckComplete()
        {
            //Views.Shell.HamburgerMenu.IsEnabled = false;
            DatabaseSystem.Instance.cleanTables();
            listTask = new List<Task>();
            await StartRecensioni();
            await StartSoluzioni();
            await StartPodcast();
            await Task.WhenAll(listTask).ContinueWith((t) =>
            {

                WindowWrapper.Current().Dispatcher.Dispatch(async () =>
                {
                    Settings.Instance.FirstStart = false;
                    AdventuresPlanetManager.Instance.Load();
                    await Task.Delay(1000);
                    Dispose();
                    NavigationService.Navigate(typeof(Views.NewsPage));
                    //Views.Shell.HamburgerMenu.IsEnabled = true;
                });
            });
        }
        public async Task StartRecensioni()
        {
            Task t = AdventuresPlanetManager.Instance.initRecensioniFromJsonFile();
            listTask.Add(t);
            Task t1 = t.ContinueWith((res) =>
            {
                WindowWrapper.Current().Dispatcher.Dispatch(() =>
                {
                    IsRecensioniLoaded = true;
                });
            });
            listTask.Add(t1);
        }
        public async Task StartSoluzioni()
        {
            Task t = AdventuresPlanetManager.Instance.initSoluzioniFromJsonFile();
            listTask.Add(t);
            Task t1 = t.ContinueWith((res) =>
            {
                WindowWrapper.Current().Dispatcher.Dispatch(() =>
                {
                    IsSoluzioniLoaded = true;
                });
            });
            listTask.Add(t1);
        }
        public async Task StartPodcast()
        {
            Task t = AdventuresPlanetManager.Instance.initPodcastFromJsonFile();
            listTask.Add(t);
            Task t1 = t.ContinueWith((res) =>
            {
                WindowWrapper.Current().Dispatcher.Dispatch(() =>
                {
                    IsPodcastLoaded = true;
                });
            });
            listTask.Add(t1);
        }
        public void Dispose()
        {
            listTask?.Clear();
            IsPodcastLoaded = false;
            IsRecensioniLoaded = false;
            IsSoluzioniLoaded = false;
        }
    }
}
