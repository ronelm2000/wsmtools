using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.Weiss.Tools.Utilities
{
    interface IExactCloneable<T>
    {
        public T Clone();
    }
}
