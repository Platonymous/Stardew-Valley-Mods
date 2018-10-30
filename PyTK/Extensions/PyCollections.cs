using PyTK.Types;
using System.Linq;
using System;
using System.Collections.Generic;
using xTile.ObjectModel;
using StardewModdingAPI;

namespace PyTK.Extensions
{
    public static class PyCollections : object
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;
        internal static Dictionary<string, int> indexCache = new Dictionary<string, int>();

        public static IDictionary<TKey, TValue> AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> t, TKey key, TValue value)
        {
            if (!t.ContainsKey(key))
                t.Add(key, value);
            else
                t[key] = value;

            return t;
        }

        public static IPropertyCollection AddOrReplace(this IPropertyCollection t, string key, string value)
        {
            if (!t.ContainsKey(key))
                t.Add(key, value);
            else
                t[key] = value;

            return t;
        }

        public static IDictionary<TKey, TValue> AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> t, IDictionary<TKey, TValue> dict)
        {
            foreach(KeyValuePair<TKey,TValue> k in dict)
                t.AddOrReplace(k.Key, k.Value);

            return t;
        }

        public static IList<T> AddOrReplace<T>(this List<T> t, T item)
        {
            if (!t.Contains(item))
                t.Add(item);

            return t;
        }

        public static T RemoveFromList<T>(this T t, List<object> list)
        {
            list.Remove(t);
            return t;
        }

        public static List<T> toList<TKey, TValue, T>(this IDictionary<TKey, TValue> t, Func<KeyValuePair<TKey, TValue>, T> conversion)
        {
            List<T> list = new List<T>();
            if (t.Count > 0)
                foreach (KeyValuePair<TKey, TValue> i in t)
                    if (conversion.Invoke(i) is T n)
                        list.Add(n);

            return list;
        }

        public static List<TOut> toList<TIn, TOut>(this List<TIn> t, Func<TIn,TOut> conversion)
        {
            List<TOut> list = new List<TOut>();
            foreach (TIn i in t)
                    list.Add(conversion.Invoke(i));

            return list;
        }

        public static List<TOut> toList<TIn, TOut>(this TIn[] t, Func<TIn, TOut> conversion)
        {
            return toList(t.ToList(),conversion);
        }

        public static List<T> toList<T>(this List<T> t, Func<T, bool> predicate)
        {
            List<T> list = new List<T>();
            foreach (T i in t)
                if (predicate.Invoke(i))
                    list.Add(i);

            return list;
        }

        public static Dictionary<TKey,TValue> toDictionary<TKey, TValue, T>(this List<T> t, Func<T, DictionaryEntry<TKey,TValue>> conversion)
        {
            Dictionary<TKey,TValue> dict = new Dictionary<TKey, TValue>();
            foreach (T i in t)
                if (conversion.Invoke(i) is DictionaryEntry<TKey,TValue> n)
                    dict.AddOrReplace(n.key, n.value);

            return dict;
        }

        public static Dictionary<TKey,TValue> clone<TKey, TValue>(this Dictionary<TKey,TValue> t)
        {
            return t.ToDictionary(k => k.Key, v => v.Value);
        }

        public static bool Exists<TKey, TValue>(this IDictionary<TKey, TValue> t, Func<KeyValuePair<TKey, TValue>, bool> predcate)
        {
            foreach (KeyValuePair<TKey, TValue> k in t)
                if (predcate.Invoke(k))
                    return true;
            return false;
        }

        public static KeyValuePair<TKey, TValue> Find<TKey, TValue>(this IDictionary<TKey, TValue> t, Func<KeyValuePair<TKey,TValue>,bool> predcate)
        {
            foreach (KeyValuePair<TKey, TValue> k in t)
                if (predcate.Invoke(k))
                    return k;

            return new KeyValuePair<TKey,TValue>((TKey) (object) null, (TValue) (object) null);
        }

        public static IEnumerable<KeyValuePair<TKey, TValue>> FindAll<TKey, TValue>(this Dictionary<TKey, TValue> t, Func<KeyValuePair<TKey, TValue>, bool> predcate)
        {
            foreach (KeyValuePair<TKey, TValue> k in t)
                if (predcate.Invoke(k))
                    yield return k;
        }

        public static List<T> useAll<T>(this List<T> list, Action<T> action)
        {
            list.ForEach(action);
            return list;
        }

        public static IDictionary<TKey,TValue> useAll<TKey,TValue>(this IDictionary<TKey, TValue> dict, Action<KeyValuePair<TKey,TValue>> action)
        {
            foreach (KeyValuePair<TKey,TValue> entry in dict)
                action.Invoke(entry);

            return dict;
        }

        public static string Join<TKey,TValue>(this IDictionary<TKey,TValue> t, char keySeperator = '|', char valueSeperator = '=')
        {
            return String.Join(keySeperator.ToString(), t.toList(p => p.Key + valueSeperator.ToString() + p.Value));
        }

        public static IDictionary<TKey, TValue> ToDictionary<TKey,TValue>(this string dict, string joinedString, Func<string, TKey> keyConverter, Func<string, TValue> valueConverter, char keySeperator = '|', char valueSeperator = '=')
        {
            return new List<string>(joinedString.Split(keySeperator)).toDictionary(p => new DictionaryEntry<TKey, TValue>(keyConverter(p.Split(valueSeperator)[0]), valueConverter(p.Split(valueSeperator)[1])));
        }

        public static IDictionary<string, string> ToDictionary(this string dict, string joinedString, char keySeperator = '|', char valueSeperator = '=')
        {
            return new List<string>(joinedString.Split(keySeperator)).toDictionary(p => new DictionaryEntry<string, string>(p.Split(valueSeperator)[0],p.Split(valueSeperator)[1]));
        }

        public static int getIndexByName(this IDictionary<int, string> dictionary, string name)
        {
            if (indexCache.ContainsKey(name))
                return indexCache[name];

            int found = 0;

            if (name.StartsWith("startswith:"))
                found = (dictionary.Where(d => d.Value.Split('/')[0].StartsWith(name.Split(':')[1])).FirstOrDefault()).Key;
            else if (name.StartsWith("endswith:"))
                found = (dictionary.Where(d => d.Value.Split('/')[0].EndsWith(name.Split(':')[1])).FirstOrDefault()).Key;
            else if (name.StartsWith("contains:"))
                found = (dictionary.Where(d => d.Value.Split('/')[0].Contains(name.Split(':')[1])).FirstOrDefault()).Key;
            else
                found = (dictionary.Where(d => d.Value.Split('/')[0] == name).FirstOrDefault()).Key;

            indexCache.Add(name, found);

            return found;
        }
    }
}
