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
    public string DeckName;
    [XmlElement("comments")]
    public string Comments;
    [XmlElement("zone")]
    public CockatriceDeckRatio Ratios;
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
    public string Serial;
}
