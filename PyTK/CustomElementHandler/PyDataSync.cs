using System;
using System.Collections.Generic;
using System.Linq;

namespace PyTK.CustomElementHandler
{
    public class PyDataSync : IDisposable
    {
        private Dictionary<string, string> Data { get; }

        private Netcode.NetString Field { get; }

        private string LastUpdate { get; set; } = "";

        public PyDataSync(Netcode.NetString field, string key = "", string value = "")
        {
            Data = new Dictionary<string, string>();
            if (key != "")
                Data.Add(key, value);
            Field = field;
            Field.fieldChangeEvent += SaveProperty_fieldChangeEvent;
            Update();
        }

        public PyDataSync(Netcode.NetString field, Dictionary<string,string> data)
        {
            Data = data;
            Field = field;
            Field.fieldChangeEvent += SaveProperty_fieldChangeEvent;
            Update();
        }

        private void SaveProperty_fieldChangeEvent(Netcode.NetString field, string oldValue, string newValue)
        {
            if (newValue != LastUpdate)
                ParseDataString(newValue);
        }

        private void ParseDataString(string newValue, bool update = false)
        {
            Data.Clear();
            foreach (string[] p in newValue.Split(SaveHandler.seperator).Select(s => s.Split(SaveHandler.valueSeperator)))
                Data.Add(p[0], p[1]);

            if(update)
                Update();
        }
        
        private void Update()
        {
            LastUpdate = string.Join(SaveHandler.seperator.ToString(), Data.Select(k => k.Key + SaveHandler.valueSeperator.ToString() + k.Value));
            if(Field.Value != LastUpdate)
                Field.Value = LastUpdate;
        }

        public void SetSaveData(Dictionary<string, string> data)
        {
            Data.Clear();
            foreach (string key in data.Keys)
                Data.Add(key, data[key]);

            Update();
        }

        public Dictionary<string, string> GetSaveData()
        {
            return Data;
        }

        public void Set(string key, string value, int index = 0)
        {
            UpdateData(key, value);
        }

        public void Set(string key, int value)
        {
            UpdateData(key, value.ToString());
        }

        public void Set(string key, float value)
        {
            UpdateData(key, value.ToString());
        }

        public void Set(string key, bool value)
        {
            UpdateData(key, value.ToString());
        }

        public void Set(string key, long value)
        {
            UpdateData(key, value.ToString());
        }

        public string Get(string key)
        {
            if (Data.ContainsKey(key))
                return Data[key];

            return null;
        }

        private void UpdateData(string key, string value)
        {
            if (Data.ContainsKey(key) && Data[key] != value)
            {
                Data[key] = value;
                Update();
                return;
            }
            else if (!Data.ContainsKey(key))
            {
                Data.Add(key, value);
                Update();
            }
        }

        public T Get<T>(string key)
        {
            if (Data.ContainsKey(key)
                && Data[key] is string s
                && !string.IsNullOrEmpty(s))
            {
                if (typeof(T) == typeof(string))
                    return (T)(object)s;

                if (typeof(T) == typeof(bool))
                {
                    if (bool.TryParse(s, out bool b))
                        return (T)(object)b;

                    return (T)(object)false;
                }

                if (typeof(T) == typeof(int))
                {
                    if (int.TryParse(s, out int i))
                        return (T)(object)i;

                    return (T)(object)0;
                }

                if (typeof(T) == typeof(long))
                {
                    if (long.TryParse(s, out long i))
                        return (T)(object)i;

                    return (T)(object)0L;
                }

                if (typeof(T) == typeof(float))
                {
                    if (float.TryParse(s, out float i))
                        return (T)(object)i;

                    return (T)(object)0f;
                };
            }

            return default;
        }

        public void Dispose()
        {
            Field.fieldChangeEvent -= SaveProperty_fieldChangeEvent;
        }
    }

}
