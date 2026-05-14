using Montage.Weiss.Tools.Entities.Effect;

namespace Montage.Weiss.Tools.Test.Internal;

/// <summary>
/// Smoke tests / examples showing how to construct CardEffectTree instances.
/// These serve as agentic guidelines for the expected structure of effect trees.
/// </summary>
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
                [AUTO] [Put 1 <<Music>> character in your hand into your waiting room] When this card is placed on stage in your hand,
                you may pay the cost. If you do, draw up to 1 card",
                """,
                Labels = [],
                AbilityText = "draw up to 1 card",
                PreConditionText = string.Empty,
                PostConditionText = string.Empty,
                ConditionText = "When this card is placed on stage in your hand",
                Condition = [
                    new CardEffectCondition {
                        Type = ConditionType.When,
                        ConditionText = "this card is placed on stage in your hand"
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
}
