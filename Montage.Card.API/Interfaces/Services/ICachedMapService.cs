using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Card.API.Interfaces.Services
{
    public interface ICachedMapService<K,V>
    {
        public V? this[K key] { get; set; }

        public IDictionary<K, V> GetValues(IEnumerable<K> keys);
    }
}
