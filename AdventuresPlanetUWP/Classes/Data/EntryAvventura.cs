using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AdventuresPlanetUWP.Classes.Data
{
    public class EntryAvventura : INotifyPropertyChanged
    {

        public EntryAvventura(RecensioneItem rec, SoluzioneItem sol)
        {
            Recensione = rec;
            Soluzione = sol;
        }
        public SoluzioneItem Soluzione { get; set; }
        public RecensioneItem Recensione { get; set; }
        public string Titolo
        {
            get
            {
                if (Soluzione != null)
                    return Soluzione.Titolo;
                else if (Recensione != null)
                    return Recensione.Titolo;
                else 
                    return "Cancella la cache o aggiorna l'elenco delle recensioni/soluzioni";
            }
        }
        public string Id
        {
            get
            {
                if (Soluzione != null)
                    return Soluzione.Id;
                else if (Recensione != null)
                    return Recensione.Id;
                else
                    return null;
            }
        }
        public string LinkStore
        {
            get
            {
                if (RecensionePresente)
                {
                    if (!string.IsNullOrEmpty(Recensione.LinkStore))
                        return Recensione.LinkStore;
                }
                if (SoluzionePresente)
                {
                    if (!string.IsNullOrEmpty(Soluzione.LinkStore))
                        return Soluzione.LinkStore;
                }
                return null;
            }
        }
        public Boolean RecensionePresente
        {
            get
            {
                return Recensione != null;
            }
        }
        public Boolean SoluzionePresente
        {
            get
            {
                return Soluzione != null;
            }
        }
        public bool IsPreferita
        {
            get
            {
                //return DatabaseSystem.Instance.isPreferiti(Id);
                return PreferitiManager.Instance.IsPreferita(Id);
            }
            set
            {
                if (value)
                    //DatabaseSystem.Instance.aggiungiPreferiti(Id);
                    PreferitiManager.Instance.AddPreferiti(Id);
                else
                    //DatabaseSystem.Instance.rimuoviPreferiti(Id);
                    PreferitiManager.Instance.RemovePreferiti(Id);
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string p = "")
        {
            if(!string.IsNullOrEmpty(p) && PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }
    }
}
