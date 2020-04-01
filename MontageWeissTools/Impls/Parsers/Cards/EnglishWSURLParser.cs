using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.Weiss.Tools.Impls.Parsers.Cards
{
    /// <summary>
    /// Parses results from the English site. This is done using an exploit on cardsearch that allows more than 100 cards as a single query.
    /// This being an exploit means that at some time in the future this won't work.
    /// </summary>
    public class EnglishWSURLParser : ICardSetParser
    {

        public bool IsCompatible(string urlOrFile)
        {
            return false;
        }

        public IAsyncEnumerable<WeissSchwarzCard> Parse(string urlOrFile)
        {
            throw new NotImplementedException();
        }
    }
}
