using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Card.API.Entities.Impls;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Montage.Weiss.Tools.Test;

[TestClass]
public class MergeTests
{
    private MergeVerb CreateMergeVerb() => new MergeVerb();

    private WeissSchwarzCard CreateCard(string serial) =>
        new WeissSchwarzCard
        {
            Serial = serial,
            Name = new MultiLanguageString { EN = serial, JP = serial },
            Type = CardType.Character,
            Color = CardColor.Red
        };

    [TestMethod(DisplayName = "Example Test Case from Plan")]
    public void Merge_ExampleTestCase()
    {
        var mergeVerb = CreateMergeVerb();

        var s1 = new WeissSchwarzDeck
        {
            Name = "I have some Eminence so I might as well build it ",
            Remarks = ""
        };
        s1.Ratios = new Dictionary<WeissSchwarzCard, int>(WeissSchwarzCard.SerialComparer)
        {
            { CreateCard("EIS/SX07-054"), 4 },
            { CreateCard("EIS/SX07-049"), 1 },
            { CreateCard("EIS/SX07-088"), 2 },
            { CreateCard("EIS/SX07-086"), 2 },
            { CreateCard("EIS/SX07-055"), 2 },
            { CreateCard("EIS/SX07-066"), 2 },
            { CreateCard("EIS/SX07-075"), 1 },
            { CreateCard("EIS/SX07-046"), 4 },
            { CreateCard("EIS/SX07-041"), 2 },
            { CreateCard("EIS/SX07-P09"), 3 },
            { CreateCard("EIS/SX07-040"), 3 },
            { CreateCard("EIS/SX07-047"), 4 },
            { CreateCard("EIS/SX07-061"), 2 },
            { CreateCard("EIS/SX07-071"), 2 },
            { CreateCard("EIS/SX07-091"), 3 },
            { CreateCard("EIS/SX07-T14"), 2 },
            { CreateCard("EIS/SX07-T13"), 3 },
            { CreateCard("EIS/SX07-059"), 4 },
            { CreateCard("EIS/SX07-057"), 4 }
        };

        var s2 = new WeissSchwarzDeck
        {
            Name = "Sample Addition",
            Remarks = ""
        };
        s2.Ratios = new Dictionary<WeissSchwarzCard, int>(WeissSchwarzCard.SerialComparer)
        {
            { CreateCard("EIS/SX07-054S"), 3 }
        };

        var merged = mergeVerb.Merge(s1, s2);

        Assert.AreEqual("I have some Eminence so I might as well build it ", merged.Name);
        Assert.AreEqual(1, merged.Ratios[CreateCard("EIS/SX07-054")]);
        Assert.AreEqual(3, merged.Ratios[CreateCard("EIS/SX07-054S")]);
        Assert.AreEqual(4, merged.Ratios[CreateCard("EIS/SX07-046")]);
        Assert.AreEqual(50, merged.Count);
    }

    [TestMethod(DisplayName = "Basic Merge - No Overlap")]
    public void Merge_BasicNoOverlap()
    {
        var mergeVerb = CreateMergeVerb();

        var s1 = new WeissSchwarzDeck();
        s1.Ratios = new Dictionary<WeissSchwarzCard, int>(WeissSchwarzCard.SerialComparer)
        {
            { CreateCard("ABC/01-001"), 4 },
            { CreateCard("ABC/01-002"), 2 }
        };

        var s2 = new WeissSchwarzDeck();
        s2.Ratios = new Dictionary<WeissSchwarzCard, int>(WeissSchwarzCard.SerialComparer)
        {
            { CreateCard("DEF/01-001"), 3 }
        };

        var merged = mergeVerb.Merge(s1, s2);

        Assert.IsFalse(merged.Ratios.ContainsKey(CreateCard("DEF/01-001")));
        Assert.AreEqual(4, merged.Ratios[CreateCard("ABC/01-001")]);
        Assert.AreEqual(2, merged.Ratios[CreateCard("ABC/01-002")]);
    }

    [TestMethod(DisplayName = "Foil Replacement")]
    public void Merge_FoilReplacement()
    {
        var mergeVerb = CreateMergeVerb();

        var s1 = new WeissSchwarzDeck();
        s1.Ratios = new Dictionary<WeissSchwarzCard, int>(WeissSchwarzCard.SerialComparer)
        {
            { CreateCard("ABC/01-001"), 4 }
        };

        var s2 = new WeissSchwarzDeck();
        s2.Ratios = new Dictionary<WeissSchwarzCard, int>(WeissSchwarzCard.SerialComparer)
        {
            { CreateCard("ABC/01-001S"), 2 }
        };

        var merged = mergeVerb.Merge(s1, s2);

        Assert.AreEqual(2, merged.Ratios[CreateCard("ABC/01-001")]);
        Assert.AreEqual(2, merged.Ratios[CreateCard("ABC/01-001S")]);
    }

    [TestMethod(DisplayName = "Ratio Capping - Overflow")]
    public void Merge_RatioCappingOverflow()
    {
        var mergeVerb = CreateMergeVerb();

        var s1 = new WeissSchwarzDeck();
        s1.Ratios = new Dictionary<WeissSchwarzCard, int>(WeissSchwarzCard.SerialComparer)
        {
            { CreateCard("ABC/01-001"), 2 }
        };

        var s2 = new WeissSchwarzDeck();
        s2.Ratios = new Dictionary<WeissSchwarzCard, int>(WeissSchwarzCard.SerialComparer)
        {
            { CreateCard("ABC/01-001S"), 4 }
        };

        var merged = mergeVerb.Merge(s1, s2);

        Assert.AreEqual(2, merged.Ratios[CreateCard("ABC/01-001S")]);
        Assert.IsFalse(merged.Ratios.ContainsKey(CreateCard("ABC/01-001")));
    }

    [TestMethod(DisplayName = "S2 Unique Cards - Not Added")]
    public void Merge_S2UniqueCardsNotAdded()
    {
        var mergeVerb = CreateMergeVerb();

        var s1 = new WeissSchwarzDeck();
        s1.Ratios = new Dictionary<WeissSchwarzCard, int>(WeissSchwarzCard.SerialComparer)
        {
            { CreateCard("ABC/01-001"), 4 }
        };

        var s2 = new WeissSchwarzDeck();
        s2.Ratios = new Dictionary<WeissSchwarzCard, int>(WeissSchwarzCard.SerialComparer)
        {
            { CreateCard("XYZ/99-999"), 4 }
        };

        var merged = mergeVerb.Merge(s1, s2);

        Assert.IsFalse(merged.Ratios.ContainsKey(CreateCard("XYZ/99-999")));
        Assert.AreEqual(4, merged.Ratios[CreateCard("ABC/01-001")]);
    }

    [TestMethod(DisplayName = "Multiple Cards Same Base Serial")]
    public void Merge_MultipleCardsSameBaseSerial()
    {
        var mergeVerb = CreateMergeVerb();

        var s1 = new WeissSchwarzDeck();
        s1.Ratios = new Dictionary<WeissSchwarzCard, int>(WeissSchwarzCard.SerialComparer)
        {
            { CreateCard("ABC/01-001"), 2 },
            { CreateCard("ABC/01-002"), 3 }
        };

        var s2 = new WeissSchwarzDeck();
        s2.Ratios = new Dictionary<WeissSchwarzCard, int>(WeissSchwarzCard.SerialComparer)
        {
            { CreateCard("ABC/01-001S"), 2 }
        };

        var merged = mergeVerb.Merge(s1, s2);

        Assert.AreEqual(2, merged.Ratios[CreateCard("ABC/01-001S")]);
        Assert.IsFalse(merged.Ratios.ContainsKey(CreateCard("ABC/01-001")));
        Assert.AreEqual(3, merged.Ratios[CreateCard("ABC/01-002")]);
    }

    [TestMethod(DisplayName = "Total Ratios Should Match S1 Total")]
    public void Merge_TotalRatiosMatchS1Total()
    {
        var mergeVerb = CreateMergeVerb();

        var s1 = new WeissSchwarzDeck();
        s1.Ratios = new Dictionary<WeissSchwarzCard, int>(WeissSchwarzCard.SerialComparer)
        {
            { CreateCard("ABC/01-001"), 4 },
            { CreateCard("ABC/01-002"), 2 },
            { CreateCard("ABC/01-003"), 3 }
        };

        var s2 = new WeissSchwarzDeck();
        s2.Ratios = new Dictionary<WeissSchwarzCard, int>(WeissSchwarzCard.SerialComparer)
        {
            { CreateCard("ABC/01-001S"), 3 }
        };

        var merged = mergeVerb.Merge(s1, s2);
        var s1Total = s1.Ratios.Values.Sum();

        Assert.AreEqual(s1Total, merged.Ratios.Values.Sum());
    }
}
