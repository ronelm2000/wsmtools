using CommandLine;

namespace Montage.Weiss.Tools;

internal class Options
{
    [Value(0)]
    public string URI { get; set; }
}
