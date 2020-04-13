namespace TMXLoader
{
    public class PersistentData
    {
        public string Type { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public PersistentData(string type, string key, string value)
        {
            Type = type;
            Key = key;
            Value = value;
        }
    }
}
