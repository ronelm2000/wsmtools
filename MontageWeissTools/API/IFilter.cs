using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.Weiss.Tools.API
{
    /// <summary>
    /// This interface signifies that a class that can filter out certain components from executing.
    /// </summary>
    public interface IFilter<T>
    {
        public bool IsIncluded(T item);
    }
}
