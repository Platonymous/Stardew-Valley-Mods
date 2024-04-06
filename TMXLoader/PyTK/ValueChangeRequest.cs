using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMXLoader
{
    public class ValueChangeRequest<TKey,TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }

        public TValue Fallback { get; set; }

        public ValueChangeRequest(TKey key,TValue value, TValue fallback)
        {
            this.Key = key;
            this.Value = value;
            this.Fallback = fallback;
        }
    }
}
