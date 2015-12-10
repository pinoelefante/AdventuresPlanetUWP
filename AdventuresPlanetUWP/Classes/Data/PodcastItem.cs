using System;
using System.Text.RegularExpressions;

namespace AdventuresPlanetUWP.Classes.Data
{
    public class PodcastItem
    {
        public PodcastItem(string titolo, string data, string link, int stag, int ep)
        {
            Titolo = titolo;
            Data = data;
            Link = link;
            Stagione = stag;
            Episodio = ep;
        }
        public PodcastItem(string titolo, string data, string link)
        {
            Titolo = titolo;
            Data = data;
            Link = link;
            estraiDati();
        }

        public string Titolo { get; set; } = string.Empty;
        public string TitoloBG
        {
            get
            {
                if (Titolo.StartsWith("Calavera Cafè "))
                {
                    return Titolo.Substring("Calavera Cafè ".Length);
                }
                return Titolo;
            }
        }
        public string Data { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string Immagine
        {
            get
            {
                string b = $"http://www.adventuresplanet.it/immagini/imgpodcast/podcast{getSxEp()}.jpg";
                return b;
            }
        }
        private string getSxEp()
        {
            return Stagione + "x" + (Episodio < 10 ? "0" + Episodio.ToString() : Episodio.ToString());
        }
        public string Filename
        {
            get
            {
                return Link.Substring(Link.LastIndexOf('/') + 1);
            }
        }
        public int Stagione { get; private set; }
        public int Episodio { get; private set; }
        public override string ToString()
        {
            return Titolo;
        }
        public string Descrizione { get; set; } = string.Empty;
        private void estraiDati()
        {
            //[0-9]{1,2}x[0-9]{1,2}
            string pat = "[0-9]{1,2}x[0-9]{1,2}";
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);
            Match m = r.Match(Titolo);
            if (m.Success)
            {
                Group g = m.Groups[0];
                //Debug.WriteLine("Group" + 0 + "='" + g + "'");
                string[] vals = g.Value.Split(new char[] {'x'}, StringSplitOptions.RemoveEmptyEntries);
                if (vals.Length == 2)
                {
                    Stagione = Int32.Parse(vals[0]);
                    Episodio = Int32.Parse(vals[1]);
                    //Debug.WriteLine("S" + stagione + "E" + episodio);
                }
            }
        }
    }
}
