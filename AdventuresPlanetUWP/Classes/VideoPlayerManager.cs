using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AdventuresPlanetUWP.Classes
{
    class VideoPlayerManager : INotifyPropertyChanged
    {
        public static VideoPlayerManager Instance { get; } = new VideoPlayerManager();
        private VideoPlayerManager()
        {

        }
        public List<YouTubeUri> ListVideo { get; } = new List<YouTubeUri>();
        public int CurrentIndex { get; set; } = 0;

        public event PropertyChangedEventHandler PropertyChanged, OnItemLoaded;

        public bool HasNext()
        {
            if (CurrentIndex < ListVideo.Count - 1)
                return true;
            return false;
        }
        public bool HasPrev()
        {
            if (CurrentIndex > 0)
                return true;
            return false;
        }
        public YouTubeUri CurrentItem()
        {
            return ListVideo[CurrentIndex];
        }
        public YouTubeUri Next()
        {
            if (HasNext())
                CurrentIndex++;
            else
                CurrentIndex = 0;
            return ListVideo[CurrentIndex];
        }
        public YouTubeUri Prev()
        {
            if (HasPrev())
                CurrentIndex--;
            else
                CurrentIndex = ListVideo.Count - 1;
            return ListVideo[CurrentIndex];
        }
        public bool IsPlayable()
        {
            return ListVideo.Count > 0;
        }
        public void Clear()
        {
            ListVideo.Clear();
            OnItemLoaded = null;
            _itemLoaded = false;
        }
        private bool _itemLoaded;
        public bool ItemLoaded
        {
            get
            {
                return _itemLoaded;
            }
            set
            {
                _itemLoaded = value;
                NotifyItemLoaded();
            }
        }
        private void NotifyItemLoaded([CallerMemberName] string p = null)
        {
            if (OnItemLoaded != null)
            {
                OnItemLoaded(this, new PropertyChangedEventArgs(p));
            }
        }
    }
}
