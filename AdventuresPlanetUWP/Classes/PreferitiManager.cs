using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace AdventuresPlanetUWP.Classes
{
    class PreferitiManager
    {
        private static readonly string FILE_BACKUP = "avp_prefs.bak";
        private static PreferitiManager _instance;
        public static PreferitiManager Instance {
            get
            {
                if (_instance == null)
                    _instance = new PreferitiManager();
                return _instance;
            }
        }
        private ApplicationDataContainer roaming;
        
        private PreferitiManager()
        {
            roaming = ApplicationData.Current.RoamingSettings;
        }
        public void AddPreferiti(string id)
        {
            roaming.Values[$"pref_{id}"] = true;
        }
        public void RemovePreferiti(string id)
        {
            if (IsPreferita(id))
                roaming.Values.Remove($"pref_{id}");
        }
        public void RemoveAllPreferiti()
        {
            List<string> prefs = GetPreferiti();
            foreach (var pref in prefs)
                RemovePreferiti(pref);
        }
        public bool IsPreferita(string id)
        {
            return roaming.Values.ContainsKey($"pref_{id}");
        }
        public List<string> GetPreferiti()
        {
            List<string> prefs = new List<string>();
            IEnumerable<KeyValuePair<string, object>> prefs_found = roaming.Values.Where(x => x.Key.StartsWith("pref_"));
            foreach (var p in prefs_found)
                prefs.Add(p.Key.Replace("pref_", string.Empty));
            return prefs;
        }
        public async void BackupPreferiti()
        {
            List<string> prefs = GetPreferiti();
            FolderPicker folderPicker = new FolderPicker();
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                StorageFile file = await folder.CreateFileAsync(FILE_BACKUP);
                using (var storageStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                using (StreamWriter fileStream = new StreamWriter(storageStream.AsStreamForWrite()))
                {
                    foreach (var p in prefs)
                        fileStream.WriteLine(p);
                }
            }
        }
        public async Task<bool> RecoverPreferitiFromBackup()
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add(".bak");
            StorageFile file = await filePicker.PickSingleFileAsync();
            if(file != null)
            {
                using (var storageStream = (await file.OpenReadAsync()).AsStreamForRead())
                using (var fileStream = new StreamReader(storageStream))
                {
                    while (!fileStream.EndOfStream)
                    {
                        string line = fileStream.ReadLine().Trim();
                        Debug.WriteLine("Found: "+ line);
                        if(line.Length > 0)
                            AddPreferiti(line);
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
