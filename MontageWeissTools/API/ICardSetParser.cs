using Montage.Weiss.Tools.Entities;
using System;
using System.Collections.Generic;

namespace Montage.Weiss.Tools.API
{
    public interface ICardSetParser
    {
        bool IsCompatible(IParseInfo parseInfo);
        IAsyncEnumerable<WeissSchwarzCard> Parse(string urlOrLocalFile);
    }
}
