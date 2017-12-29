namespace PyTK.Types
{
    public class DictionaryEntry<TKey, TValue>
    { 
        public TKey key;
        public TValue value;

        public DictionaryEntry(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
