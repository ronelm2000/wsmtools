using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.Weiss.Tools.API
{
    /// <summary>
    /// This interface signifies that a post-processor can be skipped when executed with T information. T will be related to its CLI counterpart.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISkippable<T>
    {
        bool IsIncluded(T info);
    }
}
