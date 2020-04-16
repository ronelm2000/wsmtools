using System.Collections.Generic;

namespace Montage.Weiss.Tools.API
{
    public interface IParseInfo
    {
        string URI { get; }
        IEnumerable<string> ParserHints { get; }
    }
}