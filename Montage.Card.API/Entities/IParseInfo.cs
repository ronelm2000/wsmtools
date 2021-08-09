using System.Collections.Generic;

namespace Montage.Card.API.Entities
{
    public interface IParseInfo
    {
        string URI { get; }
        IEnumerable<string> ParserHints { get; }
    }
}