using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.Card.API.Entities
{
    public interface IExactCloneable<T>
    {
        public T Clone();
    }
}
