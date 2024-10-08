using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.GUI.Utilities;
internal static class Strings
{
    internal static string? Or(params Func<string>[] values) // will be turned into Span/IEnumerable in .NET 9
    {
        foreach (var span in values)
        {
            var possibleResult = span();
            if (!String.IsNullOrWhiteSpace(possibleResult))
                return possibleResult;
        }
        return null;
    }
}
