using AdventuresPlanetUWP.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace AdventuresPlanetUWP.ViewModels
{
    public class OpzioniPageViewModel : Mvvm.ViewModelBase
    {
        public Settings Settings
        {
            get
            {
                return Settings.Instance;
            }
        }
        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            initIndexUpdateNews();
            initIndexQualitaYoutube();
        }
        public void cancella_dati(object sender, object e)
        {

        }
        private void settaValoreQualitaYoutube(object sender, object e)
        {

        }
        private int _indexTimeUpdate, _indexQualitaYoutube;
        public int IndexUpdateNews
        {
            get
            {
                return _indexTimeUpdate;
            }
            set
            {
                Set<int>(ref _indexTimeUpdate, value);
                AggiornaIntervalloUpdateNews();
            }
        }
        public int IndexQualitaYoutube
        {
            get
            {
                return _indexQualitaYoutube;
            }
            set
            {
                Set<int>(ref _indexQualitaYoutube, value);
                AggiornaQualitaYoutube();
            }
        }
        private void initIndexUpdateNews()
        {
            switch (Settings.TimeUpdateNews)
            {
                case 3600: //1ora
                    IndexUpdateNews = 0;
                    break;
                case 14400: //4ore
                    IndexUpdateNews = 1;
                    break;
                case 28800: //8ore
                    IndexUpdateNews = 2;
                    break;
                case 43200: //12ore
                    IndexUpdateNews = 3;
                    break;
                case 86400: //1giorno
                    IndexUpdateNews = 4;
                    break;
            }
        }
        private void AggiornaIntervalloUpdateNews()
        {
            switch (IndexUpdateNews)
            {
                case 0:
                    Settings.TimeUpdateNews = 3600;
                    break;
                case 1:
                    Settings.TimeUpdateNews = 14400;
                    break;
                case 2:
                    Settings.TimeUpdateNews = 28800;
                    break;
                case 3:
                    Settings.TimeUpdateNews = 43200;
                    break;
                case 4:
                    Settings.TimeUpdateNews = 86400;
                    break;
            }
        }
        private void initIndexQualitaYoutube()
        {
            switch (Settings.QualitaVideoMax)
            {
                case 144:
                    IndexQualitaYoutube = 0;
                    break;
                case 240:
                    IndexQualitaYoutube = 1;
                    break;
                case 360:
                    IndexQualitaYoutube = 2;
                    break;
                case 480:
                    IndexQualitaYoutube = 3;
                    break;
                case 720:
                    IndexQualitaYoutube = 4;
                    break;
                case 1080:
                    IndexQualitaYoutube = 5;
                    break;
                case 2160:
                    IndexQualitaYoutube = 6;
                    break;
            }
        }
        private void AggiornaQualitaYoutube()
        {
            switch (IndexQualitaYoutube)
            {
                case 0:
                    Settings.QualitaVideoMax = 144;
                    break;
                case 1:
                    Settings.QualitaVideoMax = 240;
                    break;
                case 2:
                    Settings.QualitaVideoMax = 360;
                    break;
                case 3:
                    Settings.QualitaVideoMax = 480;
                    break;
                case 4:
                    Settings.QualitaVideoMax = 720;
                    break;
                case 5:
                    Settings.QualitaVideoMax = 1080;
                    break;
                case 6:
                    Settings.QualitaVideoMax = 2160;
                    break;
            }
        }
    }
}
