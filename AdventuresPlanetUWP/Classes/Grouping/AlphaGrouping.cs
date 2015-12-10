using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization.Collation;

namespace AdventuresPlanetUWP.Classes.Grouping
{
    class MyGrouping<T>
    {
        private const string LabelNum = "#";
        public static Dictionary<string, List<T>> AlphaKeyGroup(List<T> items, Func<T, string> keySelector, bool sort = false)
        {
            Dictionary<string, List<T>> result = new Dictionary<string, List<T>>();
            foreach(T i in items)
            {
                string label = keySelector(i).Substring(0, 1).ToUpper();
                bool numeric = isNumeric(label);
                
                if (!result.ContainsKey(numeric ? LabelNum : label))
                    result.Add(numeric ? LabelNum : label, new List<T>());

                if (sort)
                    insertInOrderAlpha(result[numeric ? LabelNum : label], i, keySelector);
                else
                    result[numeric ? LabelNum : label].Add(i);
            }
            return result;
        }
        private static bool isNumeric(string s)
        {
            switch (s)
            {
                case "0":
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                case "8":
                case "9":
                    return true;
                default: return false;
            }
        }
        private static void insertInOrderAlpha(List<T> list, T item, Func<T, string> keySelector)
        {
            bool insert = false;
            for(int i = 0; i < list.Count && !insert; i++)
            {
                T inList = list[i];
                if (keySelector(item).CompareTo(keySelector(inList))<0)
                {
                    list.Insert(i, item);
                    insert = true;
                }
            }
            if (!insert)
                list.Add(item);
        }
    }
}
