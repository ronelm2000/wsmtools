namespace Montage.Card.API.Entities;

public interface IFetchInfo
{
    public IEnumerable<string> RIDsOrSerials { get; }
    public IEnumerable<string> Flags { get; }

}
