using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Montage.Weiss.Tools.Utilities
{
    public static class EnumerableExtensions
    {
        public static IDisposable GetDisposer<K,V>(this IDictionary<K,V> originalDictionary) where V : IDisposable
        {
            return new DictionaryDisposer<K,V>(originalDictionary);
        }

        /// <summary>
        /// Concatenates the entire string enumerable as a single contiguous string.
        /// </summary>
        /// <param name="stringEnumerable"></param>
        /// <returns></returns>
        public static string ConcatAsString(this IEnumerable<string> stringEnumerable, string separator = "")
        {
            if (stringEnumerable == null) return "";
            return stringEnumerable.DefaultIfEmpty("").Aggregate((a, b) => a + separator + b);
        }

        public static IEnumerable<T> Distinct<T,K>(this IEnumerable<T> enumerable, Func<T,K> keyFunction) where K : IEquatable<K>
        {
            return enumerable.Distinct(new PredicateEqualityComparer<T,K>(keyFunction));
        }

        private class PredicateEqualityComparer<T,K> : IEqualityComparer<T> where K : IEquatable<K>
        {
            private Func<T,K> keyFunction;

            public PredicateEqualityComparer(Func<T,K> keyFunction)
            {
                this.keyFunction = keyFunction;
            }

            public bool Equals([AllowNull] T x, [AllowNull] T y)
            {
                if (x == null) return y == null;
                var kx = keyFunction(x);
                var ky = keyFunction(y);
                if (kx == null) return ky == null;
                else return kx.Equals(ky);
            }

            public int GetHashCode([DisallowNull] T obj)
            {
                return keyFunction(obj)?.GetHashCode() ?? 0;
            }
        }

        public class DictionaryDisposer<K,V> : IDisposable where V : IDisposable
    {
            IDictionary<K, V> _original;

            public DictionaryDisposer(IDictionary<K, V> original)
            {
                this._original = original;
            }

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        foreach (var val in _original.Values)
                            val.Dispose();
                    }
                    disposedValue = true;
                }
            }


            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                Dispose(true);
            }
            #endregion
        }
    }
}
