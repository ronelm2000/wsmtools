using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Weiss.Tools.Entities.Effect;
using Montage.Weiss.Tools.Entities.Effect.Token;
using Montage.Weiss.Tools.Entities.Effect.Token.Ability;
using Montage.Weiss.Tools.Exceptions;

namespace Montage.Weiss.Tools.Test.Internal;

/// <summary>
/// Smoke tests / examples showing how to construct CardEffectTree instances.
/// These serve as agentic guidelines for the expected structure of effect trees.
/// </summary>
[TestClass]
public class CardEffectTreeTests
{
    public CardEffectTree CardEffectTree { get; } = new Montage.Weiss.Tools.Entities.Effect.CardEffectTree
    {
        Effects = [
            new ContCardEffect {
                EffectText = "[CONT] This card gets +500 power for each of your other 《Music》 characters.",
                AbilityText = "This card gets +500 power for each of your other 《Music》 characters.",
                ConditionText = string.Empty,
                Condition = [],
                Labels = [],
                Abilities = [
                    new CardEffectAbility {
                        AbilityText = "This card gets +500 power for each of your other 《Music》 characters."
                    }
                ]
            }
        ]
    };

    public CardEffectTree SecondTest { get; } = new Montage.Weiss.Tools.Entities.Effect.CardEffectTree
    {
        Effects = [
            new AutoCardEffect {
                EffectText = """
                [AUTO] [Put 1 <<Music>> character in your hand into your waiting room] When this card is placed on stage from your hand,
                you may pay the cost. If you do, draw up to 1 card",
                """,
                Labels = [],
                AbilityText = "draw up to 1 card",
                PreConditionText = string.Empty,
                PostConditionText = string.Empty,
                ConditionText = "When this card is placed on stage from your hand",
                Condition = [
                    new CardEffectCondition {
                        Type = ConditionType.When,
                        ConditionText = "this card is placed on stage from your hand"
                    }
                ],
                CostText = "put 1 <<Music>> character in your hand into your waiting room",
                Cost = [
                    new CardEffectAbility {
                        AbilityText = "Put 1 <<Music>> character in your hand into your waiting room"
                    }
                ],
                Abilities = [
                    new CardEffectAbility {
                        AbilityText = "draw up to 1 card"
                    }
                ]
            }
        ]
    };

    [TestMethod]
    public void EffectParseException_IsNotImplementedException()
    {
        var effect = new EventCardEffect { Labels = [], EffectText = "", AbilityText = "", Abilities = [] };
        var ex = new TranslationNotImplementedException("test", effect);
        Assert.IsInstanceOfType<NotImplementedException>(ex);
    }

    [TestMethod]
    public void EffectParseException_CarriesEffect()
    {
        var effect = new ContCardEffect
        {
            Labels = [],
            EffectText = "[CONT] test",
            AbilityText = "test",
            ConditionText = "",
            Condition = [],
            Abilities = []
        };
        var ex = new TranslationNotImplementedException("test", effect);
        Assert.AreSame(effect, ex.Effect);
        Assert.IsInstanceOfType<ContCardEffect>(ex.Effect);
    }

    [TestMethod]
    public void CatchAllAbilityToken_Suggestions_UnknownText()
    {
        var suggestions = CatchAllAbilityToken.GenerateSuggestions("some unknown text");
        Assert.AreEqual("", suggestions, "Unknown text with no detectable patterns should produce empty suggestions");
    }

    [TestMethod]
    public void CatchAllAbilityToken_Suggestions_DetectsConditionMarkerNara()
    {
        var suggestions = CatchAllAbilityToken.GenerateSuggestions("あなたのターン中なら、");
        Assert.IsTrue(suggestions.Contains("condition marker"), "Should detect 'なら' as condition marker");
        Assert.IsTrue(suggestions.Contains("Type.If"), "Should suggest Type.If");
    }

    [TestMethod]
    public void CatchAllAbilityToken_Suggestions_DetectsConditionMarkerToki()
    {
        var suggestions = CatchAllAbilityToken.GenerateSuggestions("このカードがリバースした時");
        Assert.IsTrue(suggestions.Contains("condition marker"), "Should detect '時' as condition marker");
        Assert.IsTrue(suggestions.Contains("Type.When"), "Should suggest Type.When");
    }

    [TestMethod]
    public void CatchAllAbilityToken_Suggestions_DetectsDurationPattern()
    {
        var suggestions = CatchAllAbilityToken.GenerateSuggestions("次の相手のターンの終わりまで");
        Assert.IsTrue(suggestions.Contains("duration"), "Should mention duration");
    }

    [TestMethod]
    public void CatchAllAbilityToken_Suggestions_DetectsYouHaPrefix()
    {
        var suggestions = CatchAllAbilityToken.GenerateSuggestions("あなたは自分の山札を上から1枚");
        Assert.IsTrue(suggestions.Contains("あなたは"), "Should mention あなたは prefix");
    }

    [TestMethod]
    public void CatchAllAbilityToken_Suggestions_DetectsMultiClause()
    {
        var text = "自分の山札の上から1枚を公開する、あなたは自分の手札を1枚選び、控え室に置く";
        var suggestions = CatchAllAbilityToken.GenerateSuggestions(text);
        Assert.IsTrue(suggestions.Contains("Multi-clause"), "Should detect multi-clause pattern");
    }

    [TestMethod]
    public void CatchAllAbilityToken_Suggestions_DetectsConditionalAbility()
    {
        var text = "そのカードのレベルが1以上なら、このカードをストック置場に置いてよい";
        var suggestions = CatchAllAbilityToken.GenerateSuggestions(text);
        Assert.IsTrue(suggestions.Contains("ConditionalAbilityText"), "Should flag conditional ability pattern");
        Assert.IsTrue(suggestions.Contains("そのカードのレベルが1以上"), "Should show condition part");
        Assert.IsTrue(suggestions.Contains("このカードをストック置場に置いてよい"), "Should show ability part");
    }

    [TestMethod]
    public void UnmatchedAbility_InheritsCardEffectAbility()
    {
        var ua = new UnmatchedAbility
        {
            AbilityText = "test text",
            IsUnmatched = true,
            Suggestions = ["hint"]
        };
        Assert.IsInstanceOfType<CardEffectAbility>(ua);
        Assert.AreEqual("test text", ua.AbilityText);
        Assert.IsTrue(ua.IsUnmatched);
        Assert.AreEqual(1, ua.Suggestions.Length);
        Assert.AreEqual("hint", ua.Suggestions[0]);
    }

    [TestMethod]
    public void CatchAllAbilityToken_Translate_ReturnsUnmatchedAbility()
    {
        var token = new CatchAllAbilityToken();
        var registry = NSubstitute.Substitute.For<ITokenRegistry>();
        var result = token.Translate(registry, "このカードのパワー".AsMemory());
        Assert.AreEqual(1, result.Count);
        Assert.IsInstanceOfType<UnmatchedAbility>(result[0]);
        var unmatched = (UnmatchedAbility)result[0];
        Assert.IsTrue(unmatched.IsUnmatched);
        Assert.IsTrue(unmatched.Suggestions.Length > 0);
        Assert.AreEqual("このカードのパワー", unmatched.AbilityText);
    }

    [TestMethod]
    public void GenerateSuggestionItems_ReturnsSparseHints()
    {
        var items = CatchAllAbilityToken.GenerateSuggestionItems("このカードのパワー").ToArray();
        Assert.IsTrue(items.Length > 0);
        Assert.IsTrue(items.Any(i => i.Contains("このカード")), "Should flag 'このカード' prefix");
    }

    [TestMethod]
    public void GenerateSuggestionItems_DetectsMultiClause()
    {
        var text = "公開する、手札に加える";
        var items = CatchAllAbilityToken.GenerateSuggestionItems(text).ToArray();
        Assert.IsTrue(items.Any(i => i.Contains("Multi-clause")), "Should flag multi-clause");
    }

    [TestMethod]
    public void GenerateSuggestionItems_SuggestsConditionalAbilityPattern()
    {
        var text = "そのカードのレベルが1以上なら、このカードをストック置場に置いてよい";
        var items = CatchAllAbilityToken.GenerateSuggestionItems(text).ToArray();
        Assert.IsTrue(items.Any(i => i.Contains("ConditionalAbilityText")),
            "Should flag conditional ability pattern");
        Assert.IsTrue(items.Any(i => i.Contains("そのカードのレベルが1以上")),
            "Should show condition clause");
        Assert.IsTrue(items.Any(i => i.Contains("このカードをストック置場に置いてよい")),
            "Should show ability clause");
    }
}
