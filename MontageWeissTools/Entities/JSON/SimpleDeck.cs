namespace Montage.Weiss.Tools.Entities.JSON;

public class SimpleDeck
{
    public string Name { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public Dictionary<string, int> Ratios { get; set; } = new();
}
