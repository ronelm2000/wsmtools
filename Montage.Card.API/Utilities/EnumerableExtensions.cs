using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Montage.Card.API.Utilities;

public static class EnumerableExtensions
{
    public static IDisposable GetDisposer<K,V>(this IDictionary<K,V> originalDictionary) where V : IDisposable
    {
        return new DictionaryDisposer<K,V>(originalDictionary);
    }

    public static IAsyncDisposable GetAsyncDisposer<K, V>(this IDictionary<K, V> originalDictionary) where V : IAsyncDisposable
    {
        return new AsyncDictionaryDisposer<K, V>(originalDictionary);
    }

    public static V? Add<K,V>(this IDictionary<K,V> dictionary, K key, V valueToAdd)
    {
        if (dictionary.TryAdd(key, valueToAdd))
            return valueToAdd;
        else
            return default(V);
    }

    public static IEnumerable<T> TakeUntil<T>(this IEnumerable<T> ienum, Predicate<T> predicateThatMustBeFalse)
        => ienum.TakeWhile(c => !predicateThatMustBeFalse(c));
    public static IEnumerable<T> TakeUntil<T>(this IEnumerable<T> ienum, Func<T, int, bool> predicateThatMustBeFalse)
        => ienum.TakeWhile((c,i) => !predicateThatMustBeFalse(c, i));

    /// <summary>
    /// Concatenates the entire string enumerable as a single contiguous string.
    /// </summary>
    /// <param name="stringEnumerable"></param>
    /// <returns></returns>
    public static string ConcatAsString(this IEnumerable<string> stringEnumerable, string separator = "")
    {
        if (stringEnumerable == null) return string.Empty;
        return stringEnumerable.DefaultIfEmpty(string.Empty).Aggregate((a, b) => a + separator + b);
    }

    public static string ConcatAsString(this ReadOnlyMemory<string> stringMemory, string separator = "", string conjunction = "and")
    {
        var stringSpan = stringMemory.Span;
        if (stringSpan.Length == 0) return string.Empty;
        if (stringSpan.Length == 1) return stringSpan[0];
        if (stringSpan.Length == 2) return $"{stringSpan[0]} {conjunction} {stringSpan[1]}";

        var firstSlice = stringMemory[0..^1];
        var lastSlice = stringSpan[^1];
        return MemoryMarshal.ToEnumerable(firstSlice).ConcatAsString(separator) + $"{separator}{conjunction} {lastSlice}";
    }

    /// <summary>
    /// A special variant for <see cref="FirstOrDefault{T}(IEnumerable{T})"/> that returns null if the enumerable is null or empty.
    /// The problem with <see cref="FirstOrDefault{T}(IEnumerable{T})"/> is that it returns the default value of the struct type if the enumerable is empty, and
    /// that can be misleading in some cases. For example, if you have an enumerable of integers and it is empty, <see cref="FirstOrDefault{T}(IEnumerable{T})"/> will return 0, 
    /// but this ensures that this will return a T? for its class, and null for its value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumerable"></param>
    /// <param name="emptyValue">Value of the struct to return if empty. The default value is null.</param>
    /// <returns></returns>
    public static T? FirstOrEmpty<T>(this IEnumerable<T> enumerable, Nullable<T> emptyValue = null) where T : struct
    {
        if (enumerable == null)
            return emptyValue;
        if (enumerable.Count() == 0)
            return emptyValue;
        else
            return enumerable.First();
    }

    /// <summary>
    /// A special variant for <see cref="FirstOrDefault{T}(IEnumerable{T})"/> that returns null if the enumerable is null or empty. The only difference is that 
    /// this will always return null, even if the enumerable itself is null or the count is empty. (Reference Types will usually always be null if empty, but this is just to be sure it does.)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumerable"></param>
    /// <returns></returns>
    public static T? FirstOrEmpty<T>(this IEnumerable<T> enumerable) where T : class
    {
        if (enumerable == null)
            return null;
        if (enumerable.Count() == 0)
            return null;
        else
            return enumerable.First();
    }

    public static IEnumerable<T> Distinct<T, K>(this IEnumerable<T> enumerable, Func<T, K> keyFunction) where K : IEquatable<K>
    {
        return enumerable.Distinct(new PredicateEqualityComparer<T, K>(keyFunction));
    }

    private class PredicateEqualityComparer<T, K> : IEqualityComparer<T> where K : IEquatable<K>
    {
        private Func<T, K> keyFunction;

        public PredicateEqualityComparer(Func<T, K> keyFunction)
        {
            this.keyFunction = keyFunction;
        }

        public bool Equals([AllowNull] T x, [AllowNull] T y)
        {
            if (x == null) return y == null;
            var kx = keyFunction(x!);
            var ky = keyFunction(y!);
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

    public class AsyncDictionaryDisposer<K, V> : IAsyncDisposable, IDisposable where V : IAsyncDisposable
    {
        IDictionary<K, V> _original;

        public AsyncDictionaryDisposer(IDictionary<K, V> original)
        {
            this._original = original;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var v in _original.Values)
                    (v as IDisposable)?.Dispose();
                //_original = null;
            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (_original is not null)
            {
                await Task  .WhenAll(_original.Values.Select(async v => await v.DisposeAsync().ConfigureAwait(false)))
                            .ConfigureAwait(false);
            }
        }
        #endregion
    }
}
