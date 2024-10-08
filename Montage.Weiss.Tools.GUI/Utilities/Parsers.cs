using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.GUI.Utilities;
internal static class Parsers
{
    /// <summary>
    /// Implements Parse, but also handles typical exceptions from the System variant, except for FormatException.
    /// </summary>
    /// <param name="text">Text to format</param>
    /// <param name="style">Style</param>
    /// <param name="provider">Provider if provided</param>
    /// <returns></returns>
    internal static int? ParseInt(ReadOnlySpan<char> text, System.Globalization.NumberStyles style = System.Globalization.NumberStyles.Integer, IFormatProvider? provider = null)
    {
        try
        {
            return int.Parse(text, style, provider);
        }
        catch (OverflowException)
        {
            return int.MaxValue;
        }
        catch (FormatException)
        {
            return null;
        } catch (ArgumentNullException)
        {
            return null;
        }
    }
}
