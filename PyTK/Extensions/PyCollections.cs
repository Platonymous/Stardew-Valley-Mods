using System;
using System.Collections.Generic;

namespace PyTK.Extensions
{
    public static class PyCollections : object
    {
        
        public static IDictionary<TKey, TValue> AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> t, TKey key, TValue value)
        {
            if (!t.ContainsKey(key))
                t.Add(key, value);
            else
                t[key] = value;
            return t;
        }

        public static T RemoveFromList<T>(this T t, List<object> list)
        {
            list.Remove(t);
            return t;
        }

        public static List<T> toList<TKey,TValue, T>(this IDictionary<TKey, TValue> t, Func<KeyValuePair<TKey,TValue>,T> conversion)
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

        public static List<T> useAll<T>(this List<T> list, Action<T> action)
        {
            foreach (T item in list)
                action.Invoke(item);

            return list;
        }

    }
}
