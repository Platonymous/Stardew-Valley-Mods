using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile;
using xTile.Tiles;
using xTile.ObjectModel;
using System.Collections;

namespace MoreMapLayers
{
    public class PropertyIntercept : IPropertyCollection
    {
        public PropertyValue this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ICollection<string> Keys => throw new NotImplementedException();

        public ICollection<PropertyValue> Values => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(string key, PropertyValue value)
        {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<string, PropertyValue> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, PropertyValue> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public void CopyFrom(IPropertyCollection propertyCollection)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, PropertyValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, PropertyValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, PropertyValue> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out PropertyValue value)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
