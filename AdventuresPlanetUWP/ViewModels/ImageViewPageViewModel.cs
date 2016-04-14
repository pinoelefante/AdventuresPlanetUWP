using AdventuresPlanetUWP.Classes;
using AdventuresPlanetUWP.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace AdventuresPlanetUWP.ViewModels
{
    public class ImageViewPageViewModel : Mvvm.ViewModelBase
    {
        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if(parameter is string)
                SingolaImmagine(parameter as string);
            else if(parameter is Dictionary<string,object>)
                MultiImmagini(parameter as Dictionary<string, object>);
            return base.OnNavigatedToAsync(parameter, mode, state);
        }
        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            Clean();
            return base.OnNavigatedFromAsync(state, suspending);
        }
        private void SingolaImmagine(string image)
        {
            _listImages = new List<string>(1);
            _listImages.Add(image);
            CurrentIndex = 0;
        }
        private void MultiImmagini(Dictionary<string, object> par)
        {
            var currIndex = Convert.ToInt32(par["index"]);
            _titolo = par["titolo"].ToString();
            _listImages = par["immagini"] as List<string>;
            CurrentIndex = currIndex;
        }
        private void CalcolaNextPrev()
        {
            if (_index > 0)
                HasPrev = true;
            else
                HasPrev = false;
            if (_index < _listImages.Count - 1)
                HasNext = true;
            else
                HasNext = false;
        }
        private void CalcolaFilename()
        {
            if (!string.IsNullOrEmpty(_currentImage))
                FileName = _currentImage.Substring(_currentImage.LastIndexOf('/') + 1);
        }

        private int _index;
        private bool _next, _prev, _loading;
        private List<string> _listImages;
        private string _currentImage, _filename, _titolo;
        public string CurrentImage
        {
            get { return _currentImage; }
            set
            {
                Set(ref _currentImage, value);
                CalcolaFilename();
            }
        }
        public string FileName
        {
            get { return _filename; }
            set { Set(ref _filename, value); }
        }
        public bool HasNext
        {
            get { return _next; }
            set { Set(ref _next, value); }
        }
        public bool HasPrev
        {
            get { return _prev; }
            set { Set(ref _prev, value); }
        }
        public bool IsLoading
        {
            get { return _loading; }
            set { Set(ref _loading, value); }
        }
        public int CurrentIndex
        {
            get { return _index; }
            set
            {
                Set(ref _index, value);
                if (_listImages != null) //null se è stato eseguito il metodo Clean
                { 
                    CalcolaNextPrev();
                    CurrentImage = _listImages[_index];
                }
            }
        }
        public void Next(object s, object e)
        {
            if (HasNext)
                CurrentIndex++;
        }
        public void Prev(object s, object e)
        {
            if (HasPrev)
                CurrentIndex--;
        }
        public void Download(object s, object e)
        {
            AdventuresPlanetManager.Instance.DownloadImage(CurrentImage, _titolo, FileName);
        }
        private void Clean()
        {
            if (_listImages != null)
            {
                _listImages.Clear();
                _listImages.Capacity = 0;
                _listImages = null;
            }
            _titolo = null;
            _currentImage = null;
            CurrentIndex = 0;
        }
    }
}
