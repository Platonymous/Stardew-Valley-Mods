using PyTK.Types;
using System;
using System.Collections.Generic;

namespace PyTK.Extensions
{
    public static class PyCollections : object
    {
        
        public static Dictionary<TKey, TValue> AddOrReplace<TKey, TValue>(this Dictionary<TKey, TValue> t, TKey key, TValue value)
        {
            if (!t.ContainsKey(key))
                t.Add(key, value);
            else
                t[key] = value;

            return t;
        }

        public static Dictionary<TKey, TValue> AddOrReplace<TKey, TValue>(this Dictionary<TKey, TValue> t, Dictionary<TKey, TValue> dict)
        {
            foreach(KeyValuePair<TKey,TValue> k in dict)
                t.AddOrReplace(k.Key, k.Value);

            return t;
        }

        public static List<T> AddOrReplace<T>(this List<T> t, T item)
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

        public static List<T> toList<TKey,TValue, T>(this Dictionary<TKey, TValue> t, Func<KeyValuePair<TKey,TValue>,T> conversion)
        {
            List<T> list = new List<T>();
            foreach (KeyValuePair<TKey, TValue> i in t)
                if (conversion.Invoke(i) is T n)
                    list.Add(n);

            return list;
        }

        public static List<TOut> toList<TIn, TOut>(this List<TIn> t, Func<TIn,TOut> conversion)
        {
            List<TOut> list = new List<TOut>();
            foreach (TIn i in t)
                if (conversion.Invoke(i) is TOut n)
                    list.Add(n);

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

        public static List<T> useAll<T>(this List<T> list, Action<T> action)
        {
            foreach (T item in list)
                action.Invoke(item);

            return list;
        }

        public static Dictionary<TKey,TValue> useAll<TKey,TValue>(this Dictionary<TKey, TValue> dict, Action<KeyValuePair<TKey,TValue>> action)
        {
            foreach (KeyValuePair<TKey,TValue> entry in dict)
                action.Invoke(entry);

            return dict;
        }

    }
}
