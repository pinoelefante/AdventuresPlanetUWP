using AdventuresPlanetUWP.Classes.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AdventuresPlanetUWP.Classes.Data
{
    public abstract class PaginaContenuti : INotifyPropertyChanged, Cleanable
    {
        public string Id { get; set; } = string.Empty;
        public string Titolo { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string AutoreText { get; set; } = string.Empty;
        public string Testo { get; set; } = string.Empty;
        public List<string> TestoRich { get; set; } = null;
        public abstract bool isVideo { get; }
        public bool isTemporary { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;

        protected string getUrlParameter(string p)
        {
            Uri link = new Uri(Link);
            string[] parametri = link.Query.Replace("&amp;", "&").Substring(1).Split(new char[] { '&' });
            for (int i = 0; i < parametri.Length; i++)
            {
                string[] kv = parametri[i].Split(new char[] { '=' });
                if (kv[0].ToLower().CompareTo(p) == 0)
                    return kv[1];
            }
            return null;
        }

        public virtual void Clean()
        {
            Testo = string.Empty;
            if (TestoRich != null)
            {
                TestoRich.Clear();
                TestoRich = null;
            }
            LinkStore = string.Empty;
        }
        public virtual bool IsSaveable()
        {
            if (isTemporary || isVideo)
                return false;
            return true;
        }
        private string _store = string.Empty;
        public string LinkStore
        {
            get
            {
                return _store;
            }
            set
            {
                _store = value;
                propertyChanged();
            }
        }
        public bool IsPreferita
        {
            get
            {
                bool r = DatabaseSystem.Instance.isPreferiti(Id);
                return r;
            }
            set
            {
                if (value)
                    DatabaseSystem.Instance.aggiungiPreferiti(Id);
                else
                    DatabaseSystem.Instance.rimuoviPreferiti(Id);
                propertyChanged();
            }
        }
        protected void propertyChanged([CallerMemberName]string p = "")
        {
            if(!string.IsNullOrEmpty(p) && PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }
    }
}
