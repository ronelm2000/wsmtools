using Montage.Weiss.Tools.Entities;
using System;
using System.Collections.Generic;

namespace Montage.Weiss.Tools.API
{
    public interface ICardSetParser
    {
        bool IsCompatible(string urlOrLocalFile);
        IAsyncEnumerable<WeissSchwarzCard> Parse(string urlOrLocalFile);
    }
}
