using Montage.Card.API.Entities;
using System;
using System.Collections.Generic;

namespace Montage.Card.API.Interfaces.Services
{
    public interface ICardSetParser<C> where C : ICard
    {
        bool IsCompatible(IParseInfo parseInfo);
        IAsyncEnumerable<C> Parse(string urlOrLocalFile);
    }
}
