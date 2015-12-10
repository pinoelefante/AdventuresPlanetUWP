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
        private bool _rec,_sol,_pod,_mesi;
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
        public bool IsMesiNewsLoaded
        {
            get
            {
                return _mesi;
            }
            set
            {
                Set<bool>(ref _mesi, value);
            }
        }
        private List<Task> listTask;
        public async Task CheckComplete()
        {
            Views.Shell.HamburgerMenu.IsEnabled = false;
            DatabaseSystem.Instance.cleanTables();
            listTask = new List<Task>();
            await StartRecensioni();
            await StartSoluzioni();
            await StartPodcast();
            await StartMesiNews();
            await Task.WhenAll(listTask).ContinueWith((t) =>
            {

                WindowWrapper.Current().Dispatcher.Dispatch(async () =>
                {
                    Settings.Instance.FirstStart = false;
                    AdventuresPlanetManager.Instance.Load();
                    await Task.Delay(1000);
                    Dispose();
                    Views.Shell.HamburgerMenu.IsEnabled = true;
                    NavigationService.Navigate(typeof(Views.NewsPage));
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
        public async Task StartMesiNews()
        {
            Task t = AdventuresPlanetManager.Instance.initMesiNewsFromJsonFile();
            listTask.Add(t);
            Task t1 = t.ContinueWith((res) =>
            {
                WindowWrapper.Current().Dispatcher.Dispatch(() =>
                {
                    IsMesiNewsLoaded = true;
                });
            });
            listTask.Add(t1);
        }
        public void Dispose()
        {
            listTask?.Clear();
            IsMesiNewsLoaded = false;
            IsPodcastLoaded = false;
            IsRecensioniLoaded = false;
            IsSoluzioniLoaded = false;
        }
    }
}
