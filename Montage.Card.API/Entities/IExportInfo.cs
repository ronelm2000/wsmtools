namespace Montage.Card.API.Entities;

public interface IExportInfo
{
    string Source { get; }
    string Destination { get; }
    string Parser { get; }
    string Exporter { get; }
    string OutCommand { get; }
    IEnumerable<string> Flags { get; }
    bool NonInteractive { get; }
    IProgress<DeckExportProgressReport> Progress { get; }
}
