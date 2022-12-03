using System.Xml.Serialization;

namespace Montage.Weiss.Tools.Entities.External.Cockatrice;

[XmlRoot(   "cockatrice_deck",
            IsNullable = false
            )]
public class CockatriceDeck
{
    [XmlAttribute("version")]
    public string Version = "1";
    [XmlElement("deckname")]
    public string DeckName = string.Empty;
    [XmlElement("comments")]
    public string Comments = string.Empty;
    [XmlElement("zone")]
    public CockatriceDeckRatio Ratios = new CockatriceDeckRatio();
}

[XmlType("zone")]
public class CockatriceDeckRatio
{
    [XmlAttribute("name")]
    public string Name = "main";
    [XmlElement("card")]
    public List<CockatriceSerialAmountPair> Ratios = new List<CockatriceSerialAmountPair>();
}

public class CockatriceSerialAmountPair
{
    [XmlAttribute("number")]
    public int Amount;
    [XmlAttribute("name")]
    public string Serial = string.Empty;
}
