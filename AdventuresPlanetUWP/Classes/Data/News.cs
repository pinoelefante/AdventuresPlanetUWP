using System.Collections.Generic;
using System.ComponentModel;

namespace AdventuresPlanetUWP.Classes.Data
{
    public class News : INotifyPropertyChanged
    {   
        public event PropertyChangedEventHandler PropertyChanged;
        public int Id { get; set; }
        public string Titolo { get; set; } = string.Empty;
        public string AnteprimaNews { get; set; } = string.Empty;
        public string CorpoNews { get; set; } = string.Empty;
        public string DataPubblicazione { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string Immagine { get; set; } = string.Empty;
        public List<string> CorpoNewsRich { get; set; }
        public string MeseLink { get; set; }

        private void propertyChanged(string p)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(p));
        }

        public void clear()
        {
            CorpoNews = string.Empty;
            if (CorpoNewsRich != null)
            {
                CorpoNewsRich.Clear();
                CorpoNewsRich = null;
            }
        }
    }
}
