namespace AdventuresPlanetUWP.Classes.Data
{
    public class SoluzioneItem : PaginaContenuti
    {
        public SoluzioneItem(string nome, string autore, string link)
        {
            Titolo = nome;
            AutoreText = autore;
            Link = link;
            Id = getUrlParameter("game");
        }
        public override bool isVideo
        {
            get
            {
                string par = getUrlParameter("cont");
                if (par != null && (par.CompareTo("Video%20Soluzione") == 0 || par.CompareTo("Video Soluzione") == 0))
                    return true;
                return false;
            }
        }
    }
}
