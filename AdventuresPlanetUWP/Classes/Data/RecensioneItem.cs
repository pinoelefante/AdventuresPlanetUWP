using System;
using System.ComponentModel;
using System.Linq;

namespace AdventuresPlanetUWP.Classes.Data
{
    public class RecensioneItem : PaginaContenuti
    {
        public RecensioneItem(string titolo, string autore, string voto, string link)
        {
            Titolo = titolo;
            AutoreText = autore;
            VotoText = voto;
            //Debug.WriteLine($"VoteText = {VotoText} VotoInt = {VotoInt}");
            Link = link;
            Id = getUrlParameter("game");
        }
        private string _vt = string.Empty, _vut = string.Empty;
        public string VotoText { get { return _vt; } set { _vt = value; propertyChanged(); } }
        public string VotoUtentiText { get { return _vut; } set { _vut = value; propertyChanged(); } }
        public string TestoBreve { get; set; } = string.Empty;
        public bool IsRecensioneBreve
        {
            get
            {
                if (string.IsNullOrEmpty(Testo) && !string.IsNullOrEmpty(TestoBreve))
                    return true;
                return false;
            }
        }
        public override bool isVideo
        {
            get
            {
                string par = getUrlParameter("cont");
                if (par != null && par.CompareTo("Video%20Recensione") == 0)
                    return true;
                return false;
            }
        }
        public override void Clean()
        {
            base.Clean();
            VotoUtentiText = string.Empty;
            TestoBreve = string.Empty;
        }
        public override bool IsSaveable()
        {
            if (base.IsSaveable() && !IsRecensioneBreve)
                return true;
            return false;
        }
        public int VotoInt
        {
            get
            {
                if (!string.IsNullOrEmpty(VotoText))
                {
                    string number = VotoText;
                    if (number.Contains('/'))
                        number = number.Split(new char[] {'/'})[0];
                    int res = 0;
                    if(Int32.TryParse(number, out res))
                    {
                        return res;
                    }
                }
                return 0;
            }
        }
    }
}
