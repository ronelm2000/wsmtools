namespace Montage.Weiss.Tools.Entities.Effect;

internal record CardEffectTree
{
    public required List<CardEffect> Effects { get; set; }
}

internal abstract record CardEffect
{
    public abstract string Type { get; }
    public required string EffectText { get; set; }
    public required string AbilityText { get; set; }
    public required List<CardEffectAbility> Abilities { get; init; }
}

internal record AutoCardEffect : CardEffect, ICostedCardEffect, IConditionalCardEffect
{
    public override string Type => "Auto";
    public required string ConditionText { get; init; }
    public string CostText { get; init; } = string.Empty;
    public required List<CardEffectAbility> Cost { get; init; }
    public required List<CardEffectCondition> Condition { get; init; }
}

internal record ContCardEffect : CardEffect, IConditionalCardEffect
{
    public override string Type => "Cont";
    public required string ConditionText { get; init; }
    public required List<CardEffectCondition> Condition { get; init; }
}

internal record ActCardEffect : CardEffect, ICostedCardEffect
{
    public override string Type => "Act";
    public required string CostText { get; init; }
    public required List<CardEffectAbility> Cost { get; init; }
}

internal record CardEffectCondition
{
    public required string ConditionText { get; init; }
}

internal record CardEffectAbility
{
    public required string AbilityText { get; init; }
}

internal interface IConditionalCardEffect
{
    public string ConditionText { get; init; }
    public List<CardEffectCondition> Condition { get; init; }
}

internal interface ICostedCardEffect
{
    public string CostText { get; init; }
    public List<CardEffectAbility> Cost { get; init; }
}

internal class Test
{
    public CardEffectTree CardEffectTree { get; } = new Montage.Weiss.Tools.Entities.Effect.CardEffectTree
    {
        Effects = [
            new ContCardEffect {
                EffectText = "[CONT] This card gets +500 power for each of your other 《Music》 characters.",
                AbilityText = "This card gets +500 power for each of your other 《Music》 characters.",
                ConditionText = String.Empty,
                Condition = [],
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
                [AUTO] [Put 1 <<Music>> character from your hand into your waiting room] When this card is placed on stage from your hand,
                you may pay the cost. If you do, draw up to 1 card",
                """,
                AbilityText = "draw up to 1 card",
                ConditionText = "this card is placed on stage from your hand",
                Condition = [
                    new CardEffectCondition {
                        ConditionText = "this card is placed on stage from your hand"
                    }
                ],
                CostText = "put 1 <<Music>> character from your hand into your waiting room",
                Cost = [
                    new CardEffectAbility {
                        AbilityText = "Put 1 <<Music>> character from your hand into your waiting room"
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