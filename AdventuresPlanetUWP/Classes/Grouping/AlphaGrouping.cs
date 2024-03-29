﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization.Collation;

namespace AdventuresPlanetUWP.Classes.Grouping
{
    class MyGrouping<T>
    {
        private const string LabelNum = "#";
        public static Dictionary<string, List<T>> AlphaKeyGroup(List<T> items, Func<T, string> keySelector, bool sort = false, string emptyKey = "", bool eliminaDuplicati = false)
        {
            Dictionary<string, List<T>> result = new Dictionary<string, List<T>>();
            foreach(T i in items)
            {
                string param = keySelector(i);
                string label = string.Empty;
                if (param == null || param.Length == 0)
                    label = emptyKey;
                else
                    label = param.Substring(0, 1).ToUpper();
                bool numeric = isNumeric(label);
                
                if (!result.ContainsKey(numeric ? LabelNum : label))
                    result.Add(numeric ? LabelNum : label, new List<T>());

                if (sort)
                    insertInOrderAlpha(result[numeric ? LabelNum : label], i, keySelector, eliminaDuplicati);
                else
                {
                    if (eliminaDuplicati)
                    {
                        if (result[numeric ? LabelNum : label].Where(x => keySelector(x).Equals(keySelector(i)))?.Count() > 0)
                            continue;
                        else
                            result[numeric ? LabelNum : label].Add(i);
                    }
                    else
                        result[numeric ? LabelNum : label].Add(i);
                }
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
        private static void insertInOrderAlpha(List<T> list, T item, Func<T, string> keySelector, bool eliminaDuplicati = false)
        {
            bool insert = false;
            for(int i = 0; i < list.Count && !insert; i++)
            {
                T inList = list[i];
                int compare = keySelector(item).CompareTo(keySelector(inList));
                if (compare == 0 && eliminaDuplicati)
                    insert = true;
                else if (compare < 0)
                {
                    list.Insert(i, item);
                    insert = true;
                }
            }
            if (!insert)
                list.Add(item);
        }
        public static Dictionary<string, List<T>> NumericKeyGroup(List<T> items, Func<T, int> keySelector, Func<T, string> keySelString, bool sort = false, bool eliminaDuplicati = false)
        {
            Dictionary<string, List<T>> result = new Dictionary<string, List<T>>();

            foreach(T item in items)
            {
                int num = keySelector(item);
                if (!result.ContainsKey(num.ToString()))
                {
                    Debug.WriteLine("Aggiungo la chiave " + num);
                    result.Add(num.ToString(), new List<T>());
                }
                if (sort)
                    insertInOrderAlpha(result[num.ToString()], item, keySelString, eliminaDuplicati);
                else
                {
                    if (eliminaDuplicati)
                    {
                        if (result[num.ToString()].Where(x => keySelector(x).Equals(keySelector(item))).Count() > 0)
                            continue;
                        else
                            result[num.ToString()].Add(item);
                    }
                    else
                        result[num.ToString()].Add(item);
                }
                    
            }
            //return result;
            return result.OrderByDescending(x => x.Key).ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
        }
        public static Dictionary<string, List<T>> StringKeyGroup(List<T> items, Func<T, string> keySelector, bool sort = false, string emptyKey = "", bool eliminaDuplicati = false)
        {
            Dictionary<string, List<T>> result = new Dictionary<string, List<T>>();
            foreach (T i in items)
            {
                string param = keySelector(i);
                string label = string.Empty;
                if (param == null || param.Length == 0)
                    label = emptyKey;
                else
                    label = param.ToUpper();

                if (!result.ContainsKey(label))
                    result.Add(label, new List<T>());

                if (sort)
                    insertInOrderAlpha(result[label], i, keySelector, eliminaDuplicati);
                else
                {
                    if (eliminaDuplicati)
                    {
                        if (result[label].Where(x => keySelector(x).Equals(keySelector(i)))?.Count() > 0)
                            continue;
                        else
                            result[label].Add(i);
                    }
                    else
                        result[label].Add(i);
                }
            }
            return result.OrderBy(x => x.Key).ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
        }
    }
}
